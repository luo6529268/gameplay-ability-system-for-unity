using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Simulation;

namespace NTSD.DatParser
{
    internal static class LoganCombatRecordDecoder
    {
        internal static LF2ArmorData Armor(Lf2DatBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            var armor = new LF2ArmorData();
            int ptype = 0;
            bool validType = false;
            foreach (var field in block.Properties)
            {
                int value = LoganNumericDecoder.ParseInt32OrZero(field.Value);
                int cursor = 0;
                switch (field.Key)
                {
                    case "type": validType = LoganNumericDecoder.TryParseInt32(field.Value, out armor.type); break;
                    case "ptype": ptype = value; break;
                    case "ratio": armor.ratio = value; break;
                    case "decrease": armor.decrease = value; break;
                    case "mp": armor.mp = value; break;
                    case "fall": armor.fall = value; break;
                    case "bdefend": armor.bdefend = value; break;
                    case "injury": armor.injury = value; break;
                    case "spark": armor.spark = value; break;
                    case "hp": armor.hp = value; break;
                    case "recover": armor.recover = value; break;
                    case "facing": armor.facing = value; break;
                    case "action": armor.action = value; break;
                    case "reserve": armor.reserve = value; break;
                    case "delay": armor.delay = value; break;
                    case "sound1": armor.sound1 = field.Value; break;
                    case "sound2": armor.sound2 = field.Value; break;
                    case "frame":
                        if (TryReadArmorInteger(field.Value, ref cursor, out int first) &&
                            TryReadArmorInteger(field.Value, ref cursor, out int last))
                            armor.frame_ranges.Add(new LF2ArmorFrameRange { first = first, last = last });
                        break;
                    case "state": if (TryReadArmorInteger(field.Value, ref cursor, out value)) armor.states.Add(value); break;
                    case "kind": if (TryReadArmorInteger(field.Value, ref cursor, out value)) armor.kinds.Add(value); break;
                    case "id": if (TryReadArmorInteger(field.Value, ref cursor, out value)) armor.ids.Add(value); break;
                    case "effect": if (TryReadArmorInteger(field.Value, ref cursor, out value)) armor.effects.Add(value); break;
                }
            }
            if (!validType) armor.type = ptype;
            return armor;
        }

        // Armor lists use formatted stream extraction; scalar fields use strict from_chars.
        private static bool TryReadArmorInteger(string text, ref int cursor, out int value)
        {
            value = 0;
            if (text == null) return false;
            while (cursor < text.Length && (text[cursor] == ' ' || text[cursor] == '\t' ||
                text[cursor] == '\r' || text[cursor] == '\n' || text[cursor] == '\v' || text[cursor] == '\f')) cursor++;
            if (cursor == text.Length) return false;
            int start = cursor;
            if (text[cursor] == '+') start = ++cursor;
            else if (text[cursor] == '-') cursor++;
            int digits = cursor;
            while (cursor < text.Length && text[cursor] >= '0' && text[cursor] <= '9') cursor++;
            return cursor != digits && LoganNumericDecoder.TryParseInt32(text.Substring(start, cursor - start), out value);
        }

        internal static int Int32(Lf2DatSubBlock block, string key)
        {
            TryGetLastInt32(block, key, out int value);
            return value;
        }

        internal static WeaponPoint WeaponPoint(Lf2DatSubBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            var point = new WeaponPoint();
            foreach (Lf2DatProperty property in block.Properties)
            {
                int value = LoganNumericDecoder.ParseInt32OrZero(property.Value);
                switch (property.Key)
                {
                    case "kind": point.kind = value; break;
                    case "x": point.x = value; break;
                    case "y": point.y = value; break;
                    case "weaponact": point.weaponact = value; break;
                    case "attacking": point.attacking = value; break;
                    case "cover": point.cover = value; break;
                    case "dvx": point.dvx = value; break;
                    case "dvy": point.dvy = value; break;
                    case "dvz": point.dvz = value; break;
                    default: continue;
                }
                point.rawProperties[property.Key] = property.Value;
            }
            return point;
        }

        internal static BloodPoint BloodPoint(Lf2DatSubBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            var point = new BloodPoint();
            foreach (Lf2DatProperty property in block.Properties)
            {
                switch (property.Key)
                {
                    case "x": point.x = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "y": point.y = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    default: continue;
                }
                point.rawProperties[property.Key] = property.Value;
            }
            return point;
        }

        internal static InteractionArea Interaction(Lf2DatSubBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            var area = new InteractionArea { caughtact = new int[1], catchingact = new int[1] };
            ApplyInteractionGeometry(block, area);
            foreach (Lf2DatProperty property in block.Properties)
            {
                // Alignment contract: NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001; native action values have one integer.
                switch (property.Key)
                {
                    case "kind": break;
                    case "x": break;
                    case "y": break;
                    case "w": break;
                    case "h": break;
                    case "dvx": area.dvx = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dvy": area.dvy = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "fall": area.fall = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "arest": area.arest = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "vrest": area.vrest = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "respond": area.respond = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "effect": area.effect = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "drain": area.drain = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "spark": area.spark = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "recover": area.recover = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dbdefend": area.dbdefend = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "bdefend": area.bdefend = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "injury": area.injury = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "zwidth": break;
                    case "z": break;
                    case "dvz": area.dvz = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "sound": area.sound = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "cover": area.cover = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "caughtact": area.caughtact = new[] { LoganNumericDecoder.ParseFirstInt32OrZero(property.Value) }; break;
                    case "catchingact": area.catchingact = new[] { LoganNumericDecoder.ParseFirstInt32OrZero(property.Value) }; break;
                    case "pickedact": area.pickedact = LoganNumericDecoder.ParseFirstInt32OrZero(property.Value); break;
                    case "pickingact": area.pickingact = LoganNumericDecoder.ParseFirstInt32OrZero(property.Value); break;
                    case "delay": area.delay = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "poison": area.poison = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "confus": area.confus = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "weak": area.weak = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "manacle": area.manacle = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "join": area.join = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "mimic": area.mimic = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "bound": area.bound = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "facing": area.facing = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dx": area.dx = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dy": area.dy = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dz": area.dz = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "gain": area.gain = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    default: continue;
                }
                area.rawProperties[property.Key] = property.Value;
            }
            return area;
        }

        internal static WeaponStrengthEntry WeaponStrength(int index, IReadOnlyList<Lf2DatProperty> fields)
        {
            if (index < 1 || index > 9) throw new ArgumentOutOfRangeException(nameof(index));
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            var row = new WeaponStrengthEntry { index = index };
            foreach (Lf2DatProperty property in fields)
            {
                switch (property.Key)
                {
                    case "dvx": row.dvx = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dvy": row.dvy = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "fall": row.fall = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "arest": row.arest = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "vrest": row.vrest = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "respond": row.respond = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "effect": row.effect = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "drain": row.drain = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "spark": row.spark = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "recover": row.recover = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dbdefend": row.dbdefend = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "bdefend": row.bdefend = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "injury": row.injury = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "zwidth": row.zwidth = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "z": row.z = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dvz": row.dvz = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "sound": row.sound = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "cover": row.cover = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "caughtact": row.caughtact = LoganNumericDecoder.ParseFirstInt32OrZero(property.Value); break;
                }
            }
            return row;
        }

        internal static BattleBodyBoxValue BodyBox(Lf2DatSubBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            return ReadGeometry(block);
        }

        internal static void ApplyInteractionGeometry(Lf2DatSubBlock block, InteractionArea target)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            if (target == null) throw new ArgumentNullException(nameof(target));
            BattleBodyBoxValue geometry = ReadGeometry(block);
            TryGetLastInt32(block, "kind", out target.kind);
            target.x = geometry.X;
            target.y = geometry.Y;
            target.w = geometry.W;
            target.h = geometry.H;
            target.zwidth = geometry.ZWidth;
            target.hasGeometry = geometry.HasGeometry;
            TryGetLastInt32(block, "z", out target.z);
        }

        private static BattleBodyBoxValue ReadGeometry(Lf2DatSubBlock block)
        {
            // Alignment contract: NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001; failed int admission is not explicit zero.
            bool hasX = TryGetLastInt32(block, "x", out int x);
            bool hasY = TryGetLastInt32(block, "y", out int y);
            bool hasW = TryGetLastInt32(block, "w", out int w);
            bool hasH = TryGetLastInt32(block, "h", out int h);
            TryGetLastInt32(block, "zwidth", out int zWidth);
            return new BattleBodyBoxValue(x, y, w, h, zWidth, hasX && hasY && hasW && hasH);
        }

        private static bool TryGetLastInt32(Lf2DatSubBlock block, string key, out int value)
        {
            for (int index = block.Properties.Count - 1; index >= 0; index--)
            {
                Lf2DatProperty property = block.Properties[index];
                if (property.Key == key)
                    return LoganNumericDecoder.TryParseInt32(property.Value, out value);
            }
            value = 0;
            return false;
        }

        internal static BattleObjectPointValue ObjectPoint(Lf2DatSubBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            var point = new ObjectPoint();
            foreach (Lf2DatProperty property in block.Properties)
            {
                // Alignment contract: NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001; source_line is not content.
                switch (property.Key)
                {
                    case "kind": point.kind = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "x": point.x = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "y": point.y = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "z": point.z = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "action": point.action = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dvx": point.dvx = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dvy": point.dvy = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dvz": point.dvz = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "oid": point.oid = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "facing": point.facing = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "hp": point.hp = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "mp": point.mp = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "team": point.team = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "reserve": point.reserve = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "effect": point.effect = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "pic": point.pic = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "centerx": point.centerx = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "centery": point.centery = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "centerz": point.centerz = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "framea": point.framea = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "attacking": point.attacking = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "join": point.join = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "join_reserve": point.join_reserve = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "join_pic": point.join_pic = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                }
            }
            return BattleObjectPointValueAdapter.FromLegacyTask(point);
        }

        internal static CatchPoint CatchPoint(Lf2DatSubBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            var point = new CatchPoint();
            foreach (Lf2DatProperty property in block.Properties)
            {
                // Alignment contract: NTSD28-Q05-CPOINT27-CONTENT-CONTRACT-001; native keys are exact and last-win.
                switch (property.Key)
                {
                    case "kind": point.kind = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "x": point.x = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "y": point.y = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "injury": point.injury = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "cover": point.cover = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "vaction": point.vaction = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "aaction": point.aaction = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "jaction": point.jaction = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "daction": point.daction = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "taction": point.taction = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "faction": point.faction = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "baction": point.baction = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "uzaction": point.uzaction = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dzaction": point.dzaction = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "throwvx": point.throwvx = LoganNumericDecoder.ParseFiniteFloat32OrZero(property.Value); break;
                    case "throwvy": point.throwvy = LoganNumericDecoder.ParseFiniteFloat32OrZero(property.Value); break;
                    case "hurtable": point.hurtable = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "fronthurtact": point.fronthurtact = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "backhurtact": point.backhurtact = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "decrease": point.decrease = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "dircontrol": point.dircontrol = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "throwinjury": point.throwinjury = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "throwvz": point.throwvz = LoganNumericDecoder.ParseFiniteFloat32OrZero(property.Value); break;
                    case "z": point.z = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "recover": point.recover = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "drain": point.drain = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    case "gain": point.gain = LoganNumericDecoder.ParseInt32OrZero(property.Value); break;
                    default: continue;
                }
                point.rawProperties[property.Key] = property.Value;
            }
            return point;
        }
    }
}
