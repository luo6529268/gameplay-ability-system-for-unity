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
            return ConvertFrameDataCore(frameBlock, false);
        }

        public static LF2FrameData ConvertLoganFrameData(Lf2FrameBlock frameBlock)
        {
            return ConvertFrameDataCore(frameBlock, true);
        }

        private static LF2FrameData ConvertFrameDataCore(Lf2FrameBlock frameBlock, bool loganContent)
        {
            if (frameBlock == null)
                return null;

            LF2FrameData frameData = new LF2FrameData
            {
                frameId = frameBlock.FrameIndex,
                frameName = frameBlock.FrameName ?? "",
                wait = loganContent ? 0 : 1,
                UsesLoganFrameNumbers = loganContent
            };
            var bloodPoints = new List<BattleBloodPointValue>();
            var catchPoints = new List<BattleCatchPointValue>();
            var sounds = loganContent ? new List<string>() : null;

            // 转换基本属性
            foreach (var prop in frameBlock.Properties)
            {
                frameData.rawProperties[prop.Key] = prop.Value;

                int integer = loganContent ? LoganNumericDecoder.ParseInt32OrZero(prop.Value) : ParseInt(prop.Value);
                if (loganContent)
                {
                    switch (prop.Key)
                    {
                        case "centerz": frameData.centerz = integer; break;
                        case "chp": frameData.chp = integer; break;
                        case "cmp": frameData.cmp = integer; break;
                        case "dvx": frameData.nativeDvx = LoganNumericDecoder.ParseFiniteFloat64OrZero(prop.Value); break;
                        case "dvy": frameData.nativeDvy = LoganNumericDecoder.ParseFiniteFloat64OrZero(prop.Value); break;
                        case "dvz": frameData.nativeDvz = LoganNumericDecoder.ParseFiniteFloat64OrZero(prop.Value); break;
                        case "dx": frameData.dx = LoganNumericDecoder.ParseFiniteFloat64OrZero(prop.Value); break;
                        case "dy": frameData.dy = LoganNumericDecoder.ParseFiniteFloat64OrZero(prop.Value); break;
                        case "dz": frameData.dz = LoganNumericDecoder.ParseFiniteFloat64OrZero(prop.Value); break;
                        case "sound": sounds.Add(prop.Value); break;
                    }
                }

                switch (loganContent ? prop.Key : CanonicalLegacyFrameKey(prop.Key))
                {
                    case "attacking": frameData.NativePlatformAttacking = integer; break;
                    case "pic": frameData.pic = integer; break;
                    case "state": frameData.state = integer; break;
                    case "cover": frameData.cover = integer; break;
                    case "wait": frameData.wait = integer; break;
                    case "next": frameData.next = integer; break;
                    case "dvx": frameData.dvx = integer; break;
                    case "dvy": frameData.dvy = integer; break;
                    case "dvz": frameData.dvz = integer; break;
                    case "centerx": frameData.centerx = integer; break;
                    case "centery": frameData.centery = integer; break;
                    case "mp": frameData.mp = integer; break;
                    case "hp": frameData.hp = integer; break;
                    case "hit_a": frameData.hit_a = integer; break;
                    case "hit_d": frameData.hit_d = integer; break;
                    case "hit_j": frameData.hit_j = integer; break;
                    case "hit_g": frameData.hit_g = integer; break;
                    case "hit_Fj": frameData.hit_Fj = integer; break;
                    case "hit_Fa": frameData.hit_Fa = integer; break;
                    case "hit_Da": frameData.hit_Da = integer; break;
                    case "hit_Ua": frameData.hit_Ua = integer; break;
                    case "hit_ja": frameData.hit_ja = integer; break;
                    case "hit_aj": frameData.hit_aj = integer; break;
                    case "hit_ad": frameData.hit_ad = integer; break;
                    case "hit_jd": frameData.hit_jd = integer; break;
                    case "hit_Dj": frameData.hit_Dj = integer; break;
                    case "hit_Uj": frameData.hit_Uj = integer; break;
                    case "hit_f": frameData.hit_f = integer; break;
                    case "hit_b": frameData.hit_b = integer; break;
                    case "hit_uz": frameData.hit_uz = integer; break;
                    case "hit_dz": frameData.hit_dz = integer; break;
                    case "hold_a": frameData.hold_a = integer; break;
                    case "hold_d": frameData.hold_d = integer; break;
                    case "hold_j": frameData.hold_j = integer; break;
                    case "hold_f": frameData.hold_f = integer; break;
                    case "hold_b": frameData.hold_b = integer; break;
                    case "hold_uz": frameData.hold_uz = integer; break;
                    case "hold_dz": frameData.hold_dz = integer; break;
                    case "sound": frameData.sound = prop.Value; break;
                }
            }

            // 转换子块
            foreach (var subBlock in frameBlock.SubBlocks)
            {
                string subName = loganContent ? subBlock.Name : subBlock.Name.ToLower();

                switch (subName)
                {
                    case "opoint":
                        BattleObjectPointValue objectPoint =
                            loganContent ? LoganCombatRecordDecoder.ObjectPoint(subBlock) : ConvertToObjectPoint(subBlock);
                        frameData.opoints.Add(objectPoint);
                        if (!frameData.opoint.HasValue)
                        {
                            frameData.opoint = objectPoint;
                        }
                        break;

                    case "bpoint":
                        BloodPoint bloodPoint = loganContent ? LoganCombatRecordDecoder.BloodPoint(subBlock) : ConvertToBloodPoint(subBlock);
                        bloodPoints.Add(new BattleBloodPointValue(
                            bloodPoint.x,
                            bloodPoint.y));
                        if (frameData.bpoint == null)
                            frameData.bpoint = bloodPoint;
                        break;

                    case "cpoint":
                        CatchPoint catchPoint = loganContent ? LoganCombatRecordDecoder.CatchPoint(subBlock) : ConvertToCatchPoint(subBlock);
                        catchPoints.Add(
                            BattleCatchPointValueAdapter.FromLegacy(catchPoint));
                        if (frameData.cpoint == null)
                            frameData.cpoint = catchPoint;
                        break;

                    case "wpoint":
                        frameData.wpoints.Add(loganContent ? LoganCombatRecordDecoder.WeaponPoint(subBlock) : ConvertToWeaponPoint(subBlock));
                        break;

                    case "bdy":
                        if (frameData.bodies.Count == 0)
                        {
                            frameData.primaryBodyKindForEffectSuppression =
                                loganContent ? LoganCombatRecordDecoder.Int32(subBlock, "kind") : ConvertBodyKind(subBlock);
                            frameData.primaryBodyRespondForHitResponse =
                                loganContent ? LoganCombatRecordDecoder.Int32(subBlock, "respond") : ConvertBodyRespond(subBlock);
                        }
                        frameData.bodies.Add(loganContent ? LoganCombatRecordDecoder.BodyBox(subBlock) : ConvertToBodyBox(subBlock));
                        break;

                    case "itr":
                        frameData.itrs.Add(loganContent ? LoganCombatRecordDecoder.Interaction(subBlock) : ConvertToInteractionArea(subBlock));
                        break;
                }
            }

            if (loganContent) frameData.SealFrameSounds(sounds);
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

        private static string CanonicalLegacyFrameKey(string key)
        {
            string normalized = key.ToLower();
            switch (normalized)
            {
                case "hit_fj": return "hit_Fj";
                case "hit_fa": return "hit_Fa";
                case "hit_da": return "hit_Da";
                case "hit_ua": return "hit_Ua";
                case "hit_dj": return "hit_Dj";
                case "hit_uj": return "hit_Uj";
                default: return normalized;
            }
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
                BattleCatchPointValueAdapter.ValidateLegacyPropertyName(
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

            foreach (var prop in subBlock.Properties)
            {
                switch (prop.Key.ToLowerInvariant())
                {
                    case "x": x = ParseInt(prop.Value); break;
                    case "y": y = ParseInt(prop.Value); break;
                    case "w": w = ParseInt(prop.Value); break;
                    case "h": h = ParseInt(prop.Value); break;
                }
            }

            return new BattleBodyBoxValue(x, y, w, h);
        }

        private static int ConvertBodyKind(Lf2DatSubBlock subBlock)
        {
            int kind = 0;
            foreach (var prop in subBlock.Properties)
            {
                if (string.Equals(
                    prop.Key,
                    "kind",
                    StringComparison.OrdinalIgnoreCase))
                {
                    kind = ParseInt(prop.Value);
                }
            }

            return kind;
        }

        private static int ConvertBodyRespond(Lf2DatSubBlock subBlock)
        {
            int respond = 0;
            foreach (var prop in subBlock.Properties)
            {
                if (string.Equals(
                    prop.Key,
                    "respond",
                    StringComparison.OrdinalIgnoreCase))
                {
                    respond = ParseInt(prop.Value);
                }
            }

            return respond;
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
                    case "dvy":
                        itr.dvy = ParseInt(prop.Value);
                        itr.PlatformDvy = LoganNumericDecoder.ParseFiniteFloat32OrZero(prop.Value);
                        break;
                    case "dvz": itr.dvz = ParseInt(prop.Value); break;
                    case "injury": itr.injury = ParseInt(prop.Value); break;
                    case "fall": itr.fall = ParseInt(prop.Value); break;
                    case "vaction": itr.vaction = ParseInt(prop.Value); break;
                    case "arest": itr.arest = ParseInt(prop.Value); break;
                    case "vrest": itr.vrest = ParseInt(prop.Value); break;
                    case "effect": itr.effect = ParseInt(prop.Value); break;
                    case "spark": itr.spark = ParseInt(prop.Value); break;
                    case "recover": itr.recover = ParseInt(prop.Value); break;
                    case "dbdefend": itr.dbdefend = ParseInt(prop.Value); break;
                    case "kill": itr.kill = ParseInt(prop.Value); break;
                    case "bdefend": itr.bdefend = ParseInt(prop.Value); break;
                    case "attacking": itr.attacking = ParseInt(prop.Value); break;
                    case "respond": itr.respond = ParseInt(prop.Value); break;
                    case "pickingact": itr.pickingact = ParseInt(prop.Value); break;
                    case "pickedact": itr.pickedact = ParseInt(prop.Value); break;
                    case "delay": itr.delay = ParseInt(prop.Value); break;
                    case "poison": itr.poison = ParseInt(prop.Value); break;
                    case "confus": itr.confus = ParseInt(prop.Value); break;
                    case "weak": itr.weak = ParseInt(prop.Value); break;
                    case "manacle": itr.manacle = ParseInt(prop.Value); break;
                    case "join": itr.join = ParseInt(prop.Value); break;
                    case "mimic": itr.mimic = ParseInt(prop.Value); break;
                    case "bound": itr.bound = ParseInt(prop.Value); break;
                    case "facing": itr.facing = ParseInt(prop.Value); break;
                    case "dx": itr.dx = ParseInt(prop.Value); break;
                    case "dy": itr.dy = ParseInt(prop.Value); break;
                    case "dz": itr.dz = ParseInt(prop.Value); break;
                    case "gain": itr.gain = ParseInt(prop.Value); break;
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

        public static void ApplyNativeInputDefinitionData(
            Lf2DatFile datFile,
            LF2CharacterData characterData, bool loganContent = false)
        {
            if (datFile == null)
                throw new ArgumentNullException(nameof(datFile));
            if (characterData == null)
                throw new ArgumentNullException(nameof(characterData));

            StringComparison comparison = loganContent ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            int Decode(string value) => loganContent ? LoganNumericDecoder.ParseInt32OrZero(value) : ParseInt(value);

            if (datFile.Bmp != null)
            {
                foreach (Lf2DatProperty property in datFile.Bmp.Properties)
                {
                    if (string.Equals(
                        property?.Key,
                        "use_ai",
                        comparison))
                    {
                        characterData.use_ai = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "property",
                        comparison))
                    {
                        characterData.property = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "effect",
                        comparison))
                    {
                        characterData.definition_effect = Decode(property.Value);
                    }
                }

            }

            characterData.walking_frames ??= new List<int>();
            characterData.running_frames ??= new List<int>();
            characterData.heavy_walking_frames ??= new List<int>();
            characterData.heavy_running_frames ??= new List<int>();
            CopyNativeMovementSequence(
                datFile.Bmp,
                "walking_frame",
                characterData.walking_frames);
            CopyNativeMovementSequence(
                datFile.Bmp,
                "running_frame",
                characterData.running_frames);
            CopyNativeMovementSequence(
                datFile.Bmp,
                "heavy_walking_frame",
                characterData.heavy_walking_frames);
            CopyNativeMovementSequence(
                datFile.Bmp,
                "heavy_running_frame",
                characterData.heavy_running_frames);

            characterData.normal_attack1 = 0;
            characterData.normal_attack2 = 0;
            characterData.definition_attacking = 0;
            characterData.light_throw = 0;
            characterData.weapon_drink = 0;
            characterData.heavy_throw = 0;
            characterData.run_heavy_throw = 0;
            characterData.run_attack = 0;
            characterData.jump_attack = 0;
            characterData.sky_light_throw = 0;

            foreach (Lf2DatBlock block in loganContent ? new[] { datFile.LoganStats } : (IEnumerable<Lf2DatBlock>)datFile.Blocks)
            {
                if (!string.Equals(
                    block?.Name,
                    "stats",
                    comparison))
                {
                    continue;
                }

                foreach (Lf2DatProperty property in block.Properties)
                {
                    if (string.Equals(
                        property?.Key,
                        "attacking",
                        comparison))
                    {
                        characterData.definition_attacking =
                            Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "recmp",
                        comparison))
                    {
                        characterData.recmp = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "caughtact",
                        comparison))
                    {
                        characterData.caughtact = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "normal_attack1",
                        comparison))
                    {
                        characterData.normal_attack1 = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "normal_attack2",
                        comparison))
                    {
                        characterData.normal_attack2 = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "light_throw",
                        comparison))
                    {
                        characterData.light_throw = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "weapon_drink",
                        comparison))
                    {
                        characterData.weapon_drink = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "heavy_throw",
                        comparison))
                    {
                        characterData.heavy_throw = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "run_heavy_throw",
                        comparison))
                    {
                        characterData.run_heavy_throw = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "run_attack",
                        comparison))
                    {
                        characterData.run_attack = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "jump_attack",
                        comparison))
                    {
                        characterData.jump_attack = Decode(property.Value);
                    }
                    else if (string.Equals(
                        property?.Key,
                        "sky_light_throw",
                        comparison))
                    {
                        characterData.sky_light_throw = Decode(property.Value);
                    }
                }
            }
        }

        public static void ApplyNativeArmorDefinitionData(
            Lf2DatFile datFile,
            LF2CharacterData characterData, bool loganContent = false)
        {
            if (datFile == null)
                throw new ArgumentNullException(nameof(datFile));
            if (characterData == null)
                throw new ArgumentNullException(nameof(characterData));

            characterData.armors ??= new List<LF2ArmorData>();
            characterData.armors.Clear();
            if (loganContent)
            {
                if (datFile.LoganArmors == null)
                    throw new InvalidOperationException("Logan armor requires the native parser entry.");
                foreach (var armor in datFile.LoganArmors)
                    characterData.armors.Add(LoganCombatRecordDecoder.Armor(armor));
                return;
            }
            foreach (Lf2DatBlock block in datFile.Blocks)
            {
                if (string.Equals(
                    block?.Name,
                    "armor",
                    StringComparison.OrdinalIgnoreCase))
                {
                    characterData.armors.Add(ConvertToArmorData(block));
                }
            }
        }

        private static LF2ArmorData ConvertToArmorData(Lf2DatBlock block)
        {
            var result = new LF2ArmorData();
            int ptype = 0;
            bool hasType = false;
            foreach (Lf2DatProperty property in block.Properties)
            {
                string key = property?.Key?.ToLowerInvariant();
                string value = property?.Value;
                switch (key)
                {
                    case "type":
                        result.type = ParseInt(value);
                        hasType = true;
                        break;
                    case "ptype": ptype = ParseInt(value); break;
                    case "ratio": result.ratio = ParseInt(value); break;
                    case "decrease": result.decrease = ParseInt(value); break;
                    case "mp": result.mp = ParseInt(value); break;
                    case "fall": result.fall = ParseInt(value); break;
                    case "bdefend": result.bdefend = ParseInt(value); break;
                    case "injury": result.injury = ParseInt(value); break;
                    case "spark": result.spark = ParseInt(value); break;
                    case "hp": result.hp = ParseInt(value); break;
                    case "recover": result.recover = ParseInt(value); break;
                    case "facing": result.facing = ParseInt(value); break;
                    case "action": result.action = ParseInt(value); break;
                    case "reserve": result.reserve = ParseInt(value); break;
                    case "delay": result.delay = ParseInt(value); break;
                    case "sound1": result.sound1 = value; break;
                    case "sound2": result.sound2 = value; break;
                    case "frame":
                        if (TryParseFirstTwoIntegers(
                            value,
                            out int first,
                            out int last))
                        {
                            result.frame_ranges.Add(new LF2ArmorFrameRange
                            {
                                first = first,
                                last = last,
                            });
                        }
                        break;
                    case "state": AddFirstInteger(value, result.states); break;
                    case "kind": AddFirstInteger(value, result.kinds); break;
                    case "id": AddFirstInteger(value, result.ids); break;
                    case "effect": AddFirstInteger(value, result.effects); break;
                }
            }

            if (!hasType)
                result.type = ptype;
            return result;
        }

        private static void AddFirstInteger(string value, List<int> target)
        {
            if (TryParseFirstInteger(value, out int parsed))
                target.Add(parsed);
        }

        private static bool TryParseFirstTwoIntegers(
            string value,
            out int first,
            out int second)
        {
            first = 0;
            second = 0;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            string[] tokens = value.Split((char[])null,
                StringSplitOptions.RemoveEmptyEntries);
            return tokens.Length >= 2 &&
                   int.TryParse(tokens[0], out first) &&
                   int.TryParse(tokens[1], out second);
        }

        private static bool TryParseFirstInteger(string value, out int parsed)
        {
            parsed = 0;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            string[] tokens = value.Split((char[])null,
                StringSplitOptions.RemoveEmptyEntries);
            return tokens.Length > 0 && int.TryParse(tokens[0], out parsed);
        }

        private static void CopyNativeMovementSequence(
            Lf2BmpSection bmp,
            string name,
            List<int> destination)
        {
            destination.Clear();
            if (bmp?.FrameSequences == null)
                return;

            foreach (Lf2BmpFrameSequence sequence in bmp.FrameSequences)
            {
                if (!string.Equals(
                    sequence?.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (sequence.Actions != null)
                    destination.AddRange(sequence.Actions);
                return;
            }
        }
    }
}
