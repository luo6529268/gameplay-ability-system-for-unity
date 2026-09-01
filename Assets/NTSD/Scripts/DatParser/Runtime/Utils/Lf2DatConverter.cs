using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 Dat 文件数据转换器
    /// 将通用的 Lf2DatFile 转换为项目使用的 LF2FrameData 等强类型结构
    /// </summary>
    public static class Lf2DatConverter
    {
        /// <summary>
        /// 将 Lf2FrameBlock 转换为 LF2FrameData
        /// </summary>
        public static LF2FrameData ConvertToFrameData(Lf2FrameBlock frameBlock)
        {
            if (frameBlock == null)
                return null;

            LF2FrameData frameData = new LF2FrameData
            {
                frameId = frameBlock.FrameIndex,
                frameName = frameBlock.FrameName ?? ""
            };
            var bloodPoints = new List<BattleBloodPointValue>();
            var catchPoints = new List<BattleCatchPointValue>();

            // 转换基本属性
            foreach (var prop in frameBlock.Properties)
            {
                frameData.rawProperties[prop.Key] = prop.Value;

                switch (prop.Key.ToLower())
                {
                    case "pic": frameData.pic = ParseInt(prop.Value); break;
                    case "state": frameData.state = ParseInt(prop.Value); break;
                    case "wait": frameData.wait = ParseInt(prop.Value); break;
                    case "next": frameData.next = ParseInt(prop.Value); break;
                    case "dvx": frameData.dvx = ParseInt(prop.Value); break;
                    case "dvy": frameData.dvy = ParseInt(prop.Value); break;
                    case "dvz": frameData.dvz = ParseInt(prop.Value); break;
                    case "centerx": frameData.centerx = ParseInt(prop.Value); break;
                    case "centery": frameData.centery = ParseInt(prop.Value); break;
                    case "mp": frameData.mp = ParseInt(prop.Value); break;
                    case "hit_a": frameData.hit_a = ParseInt(prop.Value); break;
                    case "hit_d": frameData.hit_d = ParseInt(prop.Value); break;
                    case "hit_j": frameData.hit_j = ParseInt(prop.Value); break;
                    case "hit_fj": frameData.hit_Fj = ParseInt(prop.Value); break;
                    case "hit_fa": frameData.hit_Fa = ParseInt(prop.Value); break;
                    case "hit_da": frameData.hit_Da = ParseInt(prop.Value); break;
                    case "hit_ua": frameData.hit_Ua = ParseInt(prop.Value); break;
                    case "hit_ja": frameData.hit_ja = ParseInt(prop.Value); break;
                    case "hit_dj": frameData.hit_Dj = ParseInt(prop.Value); break;
                    case "hit_uj": frameData.hit_Uj = ParseInt(prop.Value); break;
                    case "sound": frameData.sound = prop.Value; break;
                }
            }

            // 转换子块
            foreach (var subBlock in frameBlock.SubBlocks)
            {
                string subName = subBlock.Name.ToLower();

                switch (subName)
                {
                    case "opoint":
                        BattleObjectPointValue objectPoint =
                            ConvertToObjectPoint(subBlock);
                        frameData.opoints.Add(objectPoint);
                        if (!frameData.opoint.HasValue)
                        {
                            frameData.opoint = objectPoint;
                        }
                        break;

                    case "bpoint":
                        BloodPoint bloodPoint = ConvertToBloodPoint(subBlock);
                        bloodPoints.Add(new BattleBloodPointValue(
                            bloodPoint.x,
                            bloodPoint.y));
                        if (frameData.bpoint == null)
                            frameData.bpoint = bloodPoint;
                        break;

                    case "cpoint":
                        CatchPoint catchPoint = ConvertToCatchPoint(subBlock);
                        catchPoints.Add(
                            BattleCatchPointValueAdapter.FromLegacy(catchPoint));
                        if (frameData.cpoint == null)
                            frameData.cpoint = catchPoint;
                        break;

                    case "wpoint":
                        frameData.wpoints.Add(ConvertToWeaponPoint(subBlock));
                        break;

                    case "bdy":
                        frameData.bodies.Add(ConvertToBodyBox(subBlock));
                        break;

                    case "itr":
                        frameData.itrs.Add(ConvertToInteractionArea(subBlock));
                        break;
                }
            }

            frameData.SealFormalWeaponPoints();
            frameData.SealBloodPoints(bloodPoints);
            frameData.SealCatchPoints(catchPoints);

            // 调试日志：显示转换后的帧数据摘要
            if (frameBlock.FrameIndex % 50 == 0) // 每 50 帧打印一次，避免日志过多
            {
                UnityEngine.Debug.Log($"<color=cyan>[Converter] 帧 {frameBlock.FrameIndex} ({frameBlock.FrameName}): " +
                    $"pic={frameData.pic}, state={frameData.state}, wait={frameData.wait}, next={frameData.next}, " +
                    $"bodies={frameData.bodies.Count}, itrs={frameData.itrs.Count}, wpoints={frameData.wpoints.Count}, " +
                    $"opoint={(frameData.opoint.HasValue ? "有" : "无")}, 属性数={frameBlock.Properties.Count}</color>");
            }

            return frameData;
        }

        /// <summary>
        /// 转换 ObjectPoint
        /// </summary>
        private static BattleObjectPointValue ConvertToObjectPoint(
            Lf2DatSubBlock subBlock)
        {
            int kind = 0;
            int x = 0;
            int y = 0;
            int action = 0;
            int dvx = 0;
            int dvy = 0;
            int oid = 0;
            int facing = 0;

            foreach (var prop in subBlock.Properties)
            {
                switch (prop.Key.ToLower())
                {
                    case "kind": kind = ParseInt(prop.Value); break;
                    case "x": x = ParseInt(prop.Value); break;
                    case "y": y = ParseInt(prop.Value); break;
                    case "action": action = ParseInt(prop.Value); break;
                    case "dvx": dvx = ParseInt(prop.Value); break;
                    case "dvy": dvy = ParseInt(prop.Value); break;
                    case "oid": oid = ParseInt(prop.Value); break;
                    case "facing": facing = ParseInt(prop.Value); break;
                }
            }
            return new BattleObjectPointValue(
                kind,
                x,
                y,
                action,
                dvx,
                dvy,
                oid,
                facing);
        }

        /// <summary>
        /// 转换 BloodPoint
        /// </summary>
        private static BloodPoint ConvertToBloodPoint(Lf2DatSubBlock subBlock)
        {
            BloodPoint bpoint = new BloodPoint();

            foreach (var prop in subBlock.Properties)
            {
                string propertyName = prop.Key?.ToLowerInvariant();
                if (propertyName != "x" && propertyName != "y")
                {
                    throw new InvalidOperationException(
                        $"BPoint property '{prop.Key}' is outside the formal release contract.");
                }
                bpoint.rawProperties[prop.Key] = prop.Value;

                switch (propertyName)
                {
                    case "x": bpoint.x = ParseInt(prop.Value); break;
                    case "y": bpoint.y = ParseInt(prop.Value); break;
                }
            }

            return bpoint;
        }

        /// <summary>
        /// 转换 CatchPoint
        /// </summary>
        private static CatchPoint ConvertToCatchPoint(Lf2DatSubBlock subBlock)
        {
            CatchPoint cpoint = new CatchPoint();

            foreach (var prop in subBlock.Properties)
            {
                BattleCatchPointValueAdapter.ValidateFormalPropertyName(
                    prop.Key);
                cpoint.rawProperties[prop.Key] = prop.Value;

                switch (prop.Key.ToLowerInvariant())
                {
                    case "kind": cpoint.kind = ParseInt(prop.Value); break;
                    case "x": cpoint.x = ParseInt(prop.Value); break;
                    case "y": cpoint.y = ParseInt(prop.Value); break;
                    case "fronthurtact":
                        cpoint.fronthurtact = ParseInt(prop.Value);
                        cpoint.injury = cpoint.fronthurtact;
                        break;
                    case "backhurtact":
                        cpoint.backhurtact = ParseInt(prop.Value);
                        cpoint.cover = cpoint.backhurtact;
                        break;
                    case "vaction": cpoint.vaction = ParseInt(prop.Value); break;
                    case "throwvz": cpoint.throwvz = ParseInt(prop.Value); break;
                    case "hurtable": cpoint.hurtable = ParseInt(prop.Value); break;
                    case "throwinjury": cpoint.throwinjury = ParseInt(prop.Value); break;
                    case "decrease": cpoint.decrease = ParseInt(prop.Value); break;
                    // C++ release 确认的额外字段
                    case "injury": cpoint.injury = ParseInt(prop.Value); break;
                    case "cover": cpoint.cover = ParseInt(prop.Value); break;
                    case "aaction": cpoint.aaction = ParseInt(prop.Value); break;
                    case "jaction": cpoint.jaction = ParseInt(prop.Value); break;
                    case "taction": cpoint.taction = ParseInt(prop.Value); break;
                    case "daction": cpoint.daction = ParseInt(prop.Value); break;
                    case "throwvx": cpoint.throwvx = ParseInt(prop.Value); break;
                    case "throwvy": cpoint.throwvy = ParseInt(prop.Value); break;
                    case "dircontrol": cpoint.dircontrol = ParseInt(prop.Value); break;
                }
            }

            return cpoint;
        }

        /// <summary>
        /// 转换 WeaponPoint
        /// </summary>
        private static WeaponPoint ConvertToWeaponPoint(Lf2DatSubBlock subBlock)
        {
            var wpoint = new WeaponPoint();

            foreach (var prop in subBlock.Properties)
            {
                BattleWeaponPointValueAdapter.ValidateFormalPropertyName(
                    prop.Key);
                wpoint.rawProperties[prop.Key] = prop.Value;

                switch (prop.Key.ToLowerInvariant())
                {
                    case "kind": wpoint.kind = ParseInt(prop.Value); break;
                    case "x": wpoint.x = ParseInt(prop.Value); break;
                    case "y": wpoint.y = ParseInt(prop.Value); break;
                    case "weaponact": wpoint.weaponact = ParseInt(prop.Value); break;
                    case "attacking": wpoint.attacking = ParseInt(prop.Value); break;
                    case "cover": wpoint.cover = ParseInt(prop.Value); break;
                    case "dvx": wpoint.dvx = ParseInt(prop.Value); break;
                    case "dvy": wpoint.dvy = ParseInt(prop.Value); break;
                    case "dvz": wpoint.dvz = ParseInt(prop.Value); break;
                }
            }

            return wpoint;
        }

        /// <summary>
        /// 转换 BodyBox
        /// </summary>
        private static BattleBodyBoxValue ConvertToBodyBox(Lf2DatSubBlock subBlock)
        {
            int x = 0;
            int y = 0;
            int w = 0;
            int h = 0;

            for (int pass = 0; pass < 2; pass++)
            {
                bool exactPass = pass == 0;
                foreach (var prop in subBlock.Properties)
                {
                    char propertyTag = ResolveReleaseBodyPropertyTag(
                        prop.Key,
                        out bool isExact);
                    if (propertyTag == '\0' || isExact != exactPass)
                        continue;

                    switch (propertyTag)
                    {
                        case 'x': x = ParseInt(prop.Value); break;
                        case 'y': y = ParseInt(prop.Value); break;
                        case 'w': w = ParseInt(prop.Value); break;
                        case 'h': h = ParseInt(prop.Value); break;
                    }
                }
            }

            return new BattleBodyBoxValue(x, y, w, h);
        }

        private static char ResolveReleaseBodyPropertyTag(
            string propertyName,
            out bool isExact)
        {
            isExact = false;
            if (string.IsNullOrEmpty(propertyName))
                return '\0';

            if (propertyName.Length == 1)
            {
                char exactTag = char.ToLowerInvariant(propertyName[0]);
                if (exactTag == 'x' || exactTag == 'y' || exactTag == 'w' || exactTag == 'h')
                {
                    isExact = true;
                    return exactTag;
                }
            }

            char suffix = propertyName[propertyName.Length - 1];
            return suffix == 'x' || suffix == 'y' || suffix == 'w' || suffix == 'h'
                ? suffix
                : '\0';
        }

        /// <summary>
        /// 转换 InteractionArea
        /// </summary>
        private static InteractionArea ConvertToInteractionArea(Lf2DatSubBlock subBlock)
        {
            InteractionArea itr = new InteractionArea();

            foreach (var prop in subBlock.Properties)
            {
                itr.rawProperties[prop.Key] = prop.Value;

                switch (prop.Key.ToLower())
                {
                    case "kind": itr.kind = ParseInt(prop.Value); break;
                    case "x": itr.x = ParseInt(prop.Value); break;
                    case "y": itr.y = ParseInt(prop.Value); break;
                    case "w": itr.w = ParseInt(prop.Value); break;
                    case "h": itr.h = ParseInt(prop.Value); break;
                    case "zwidth": itr.zwidth = ParseInt(prop.Value); break;
                    case "dvx": itr.dvx = ParseInt(prop.Value); break;
                    case "dvy": itr.dvy = ParseInt(prop.Value); break;
                    case "dvz": itr.dvz = ParseInt(prop.Value); break;
                    case "injury": itr.injury = ParseInt(prop.Value); break;
                    case "fall": itr.fall = ParseInt(prop.Value); break;
                    case "vaction": itr.vaction = ParseInt(prop.Value); break;
                    case "arest": itr.arest = ParseInt(prop.Value); break;
                    case "vrest": itr.vrest = ParseInt(prop.Value); break;
                    case "effect": itr.effect = ParseInt(prop.Value); break;
                    case "kill": itr.kill = ParseInt(prop.Value); break;
                    case "bdefend": itr.bdefend = ParseInt(prop.Value); break;
                    case "attacking": itr.attacking = ParseInt(prop.Value); break;
                    case "respond": itr.respond = ParseInt(prop.Value); break;
                    case "pickingact": itr.pickingact = ParseInt(prop.Value); break;
                    case "pickedact": itr.pickedact = ParseInt(prop.Value); break;
                    case "throwvx": itr.throwvx = ParseInt(prop.Value); break;
                    case "throwvy": itr.throwvy = ParseInt(prop.Value); break;
                    case "throwinjury": itr.throwinjury = ParseInt(prop.Value); break;
                    case "throwvz": itr.throwvz = ParseInt(prop.Value); break;
                    case "catchingact": itr.catchingact = ParseIntPair(prop.Value); break;
                    case "catchingact2": itr.catchingact2 = ParseIntPair(prop.Value); break;
                    case "caughtact": itr.caughtact = ParseIntPair(prop.Value); break;
                    case "caughtact2": itr.caughtact2 = ParseIntPair(prop.Value); break;
                }
            }

            return itr;
        }

        /// <summary>
        /// 解析整数。C++ release 在 Windows/MinGW 下使用 32 位 strtol；
        /// 超出范围的 DAT 数值会饱和到 int 边界，而不是变成 0。
        /// </summary>
        private static int ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            string text = value.Trim();
            int index = 0;
            int sign = 1;
            if (index < text.Length && (text[index] == '+' || text[index] == '-'))
            {
                sign = text[index] == '-' ? -1 : 1;
                index++;
            }

            long limit = sign < 0 ? 2147483648L : 2147483647L;
            long acc = 0;
            bool hasDigit = false;
            while (index < text.Length)
            {
                char ch = text[index];
                if (ch < '0' || ch > '9')
                    break;

                hasDigit = true;
                acc = acc * 10 + (ch - '0');
                if (acc >= limit)
                    return sign < 0 ? int.MinValue : int.MaxValue;

                index++;
            }

            if (!hasDigit)
                return 0;

            return sign < 0 ? unchecked((int)-acc) : (int)acc;
        }

        /// <summary>
        /// 解析两个整数
        /// </summary>
        private static int[] ParseIntPair(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string[] parts = value.Split(new[] { ' ', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return null;

            int first = ParseInt(parts[0]);
            int second = parts.Length > 1 ? ParseInt(parts[1]) : 0;
            return new[] { first, second };
        }

        /// <summary>
        /// 将整个 Lf2DatFile 转换为 LF2FrameData 列表
        /// </summary>
        public static List<LF2FrameData> ConvertAllFrames(Lf2DatFile datFile)
        {
            List<LF2FrameData> frames = new List<LF2FrameData>();

            if (datFile == null || datFile.Frames == null)
                return frames;

            foreach (var frameBlock in datFile.Frames)
            {
                LF2FrameData frameData = ConvertToFrameData(frameBlock);
                if (frameData != null)
                    frames.Add(frameData);
            }

            return frames;
        }
    }
}
