using System;
using System.Collections.Generic;
using NTSD.Animation;
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

            // 转换基本属性
            foreach (var prop in frameBlock.Properties)
            {
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
                        frameData.opoint = ConvertToObjectPoint(subBlock);
                        break;

                    case "bpoint":
                        frameData.bpoint = ConvertToBloodPoint(subBlock);
                        break;

                    case "cpoint":
                        frameData.cpoint = ConvertToCatchPoint(subBlock);
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

            // 调试日志：显示转换后的帧数据摘要
            if (frameBlock.FrameIndex % 50 == 0) // 每 50 帧打印一次，避免日志过多
            {
                UnityEngine.Debug.Log($"<color=cyan>[Converter] 帧 {frameBlock.FrameIndex} ({frameBlock.FrameName}): " +
                    $"pic={frameData.pic}, state={frameData.state}, wait={frameData.wait}, next={frameData.next}, " +
                    $"bodies={frameData.bodies.Count}, itrs={frameData.itrs.Count}, wpoints={frameData.wpoints.Count}, " +
                    $"opoint={(frameData.opoint != null ? "有" : "无")}, 属性数={frameBlock.Properties.Count}</color>");
            }

            return frameData;
        }

        /// <summary>
        /// 转换 ObjectPoint
        /// </summary>
        private static ObjectPoint ConvertToObjectPoint(Lf2DatSubBlock subBlock)
        {
            ObjectPoint opoint = new ObjectPoint();

            foreach (var prop in subBlock.Properties)
            {
                switch (prop.Key.ToLower())
                {
                    case "kind": opoint.kind = ParseInt(prop.Value); break;
                    case "action": opoint.action = ParseInt(prop.Value); break;
                    case "objectid": opoint.objectId = ParseInt(prop.Value); break;
                    case "x": opoint.x = ParseInt(prop.Value); break;
                    case "y": opoint.y = ParseInt(prop.Value); break;
                    case "dvx": opoint.dvx = ParseInt(prop.Value); break;
                    case "dvy": opoint.dvy = ParseInt(prop.Value); break;
                    case "oid": opoint.oid = ParseInt(prop.Value); break;
                    case "facing": opoint.facing = ParseInt(prop.Value); break;
                }
            }

            return opoint;
        }

        /// <summary>
        /// 转换 BloodPoint
        /// </summary>
        private static BloodPoint ConvertToBloodPoint(Lf2DatSubBlock subBlock)
        {
            BloodPoint bpoint = new BloodPoint();

            foreach (var prop in subBlock.Properties)
            {
                switch (prop.Key.ToLower())
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
                switch (prop.Key.ToLower())
                {
                    case "kind": cpoint.kind = ParseInt(prop.Value); break;
                    case "x": cpoint.x = ParseInt(prop.Value); break;
                    case "y": cpoint.y = ParseInt(prop.Value); break;
                    case "fronthurtact": cpoint.fronthurtact = ParseInt(prop.Value); break;
                    case "backhurtact": cpoint.backhurtact = ParseInt(prop.Value); break;
                    case "vaction": cpoint.vaction = ParseInt(prop.Value); break;
                    case "throwvz": cpoint.throwvz = ParseInt(prop.Value); break;
                    case "hurtable": cpoint.hurtable = ParseInt(prop.Value); break;
                    case "throwinjury": cpoint.throwinjury = ParseInt(prop.Value); break;
                    case "decrease": cpoint.decrease = ParseInt(prop.Value); break;
                    // NTSD 2.4 反汇编确认的额外字段
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
            WeaponPoint wpoint = new WeaponPoint();

            foreach (var prop in subBlock.Properties)
            {
                switch (prop.Key.ToLower())
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
        private static BodyBox ConvertToBodyBox(Lf2DatSubBlock subBlock)
        {
            BodyBox body = new BodyBox();

            foreach (var prop in subBlock.Properties)
            {
                switch (prop.Key.ToLower())
                {
                    case "kind": body.kind = ParseInt(prop.Value); break;
                    case "x": body.x = ParseInt(prop.Value); break;
                    case "y": body.y = ParseInt(prop.Value); break;
                    case "w": body.w = ParseInt(prop.Value); break;
                    case "h": body.h = ParseInt(prop.Value); break;
                }
            }

            return body;
        }

        /// <summary>
        /// 转换 InteractionArea
        /// </summary>
        private static InteractionArea ConvertToInteractionArea(Lf2DatSubBlock subBlock)
        {
            InteractionArea itr = new InteractionArea();

            foreach (var prop in subBlock.Properties)
            {
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
                    case "arest": itr.arest = ParseInt(prop.Value); break;
                    case "vrest": itr.vrest = ParseInt(prop.Value); break;
                    case "effect": itr.effect = ParseInt(prop.Value); break;
                    case "bdefend": itr.bdefend = ParseInt(prop.Value); break;
                }
            }

            return itr;
        }

        /// <summary>
        /// 解析整数
        /// </summary>
        private static int ParseInt(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            int result;
            if (int.TryParse(value, out result))
                return result;

            return 0;
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
