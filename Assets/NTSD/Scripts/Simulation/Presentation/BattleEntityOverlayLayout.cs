namespace NTSD.Simulation.Presentation
{
    public enum BattleEntityOverlayGlyphType : byte
    {
        Counter = 0,
        Label = 1,
    }

    /// <summary>
    /// A single glyph placement in a WORDS bitmap sheet.
    /// </summary>
    public struct BattleEntityOverlayGlyph
    {
        public int CharCode;
        public int SheetIndex;
        public int PixelX;
        public int PixelY;
        public int Sequence;
        public BattleEntityOverlayGlyphType Type;
    }

    /// <summary>
    /// Runtime-only values required by the BattleHostForm/SdlBattleRenderer overlay rules.
    /// </summary>
    public readonly struct BattleEntityOverlayRuntimeSlot
    {
        public BattleEntityOverlayRuntimeSlot(
            int slotIndex,
            int hp2Orig,
            int relationTeam,
            int objType,
            int oid,
            int hitStop,
            int xInt,
            int yInt,
            int zInt,
            int renderOffsetX,
            int cameraX,
            int centerY)
        {
            SlotIndex = slotIndex;
            HP2Orig = hp2Orig;
            RelationTeam = relationTeam;
            ObjType = objType;
            Oid = oid;
            HitStop = hitStop;
            XInt = xInt;
            YInt = yInt;
            ZInt = zInt;
            RenderOffsetX = renderOffsetX;
            CameraX = cameraX;
            CenterY = centerY;
        }

        public int SlotIndex { get; }
        public int HP2Orig { get; }
        public int RelationTeam { get; }
        public int ObjType { get; }
        public int Oid { get; }
        public int HitStop { get; }
        public int XInt { get; }
        public int YInt { get; }
        public int ZInt { get; }
        public int RenderOffsetX { get; }
        public int CameraX { get; }
        public int CenterY { get; }
    }

    /// <summary>
    /// Allocation-free layout shared by command and legacy overlay renderers.
    /// </summary>
    public static class BattleEntityOverlayLayout
    {
        public const int SlotCount = 10;
        public const int SlotLabelCharacterCapacity = 12;
        public const int GlyphAdvance = 9;
        public const int MaximumGlyphCount = 3 + SlotLabelCharacterCapacity + 2;

        public static bool TryBuild(
            in BattleEntityOverlayRuntimeSlot entity,
            char[,] slotLabelChars,
            int[] slotLabelState,
            BattleEntityOverlayGlyph[] glyphBuffer,
            out int glyphCount)
        {
            glyphCount = 0;
            if (slotLabelChars == null ||
                slotLabelChars.GetLength(0) < SlotCount ||
                slotLabelChars.GetLength(1) < SlotLabelCharacterCapacity ||
                slotLabelState == null ||
                slotLabelState.Length < SlotCount ||
                glyphBuffer == null)
            {
                return false;
            }

            int counterLength = entity.HP2Orig > 1 ? (entity.HP2Orig <= 9 ? 2 : 3) : 0;
            int labelLength = GetLabelLength(in entity, slotLabelChars);

            bool bracketed = entity.SlotIndex >= 0 && entity.SlotIndex < SlotCount &&
                             slotLabelState[entity.SlotIndex] == -1 &&
                             !IsSpecialCom(in entity);
            if (bracketed)
                labelLength += 2;

            int required = counterLength + labelLength;
            if (glyphBuffer.Length < required)
                return false;

            int sequence = 0;
            if (counterLength != 0)
            {
                int counterX = entity.XInt + entity.RenderOffsetX - ((GlyphAdvance * counterLength) >> 1) - entity.CameraX;
                int counterY = entity.ZInt + entity.YInt - entity.CenterY - 7;
                WriteGlyph(glyphBuffer, ref sequence, 'x', 0, counterX, counterY, BattleEntityOverlayGlyphType.Counter);
                if (counterLength == 3)
                    WriteGlyph(glyphBuffer, ref sequence, (char)('0' + ((entity.HP2Orig / 10) % 10)), 0, counterX + GlyphAdvance, counterY, BattleEntityOverlayGlyphType.Counter);
                WriteGlyph(glyphBuffer, ref sequence, (char)('0' + (entity.HP2Orig % 10)), 0, counterX + (counterLength - 1) * GlyphAdvance, counterY, BattleEntityOverlayGlyphType.Counter);
            }

            if (labelLength != 0)
            {
                int sheetIndex = IsSpecialCom(in entity) ? 5 : ResolveRelationSheet(entity.RelationTeam);
                int labelX = entity.XInt + entity.RenderOffsetX - ((GlyphAdvance * labelLength) >> 1) - entity.CameraX;
                int maxX = 794 - GlyphAdvance * labelLength;
                if (labelX < 0)
                    labelX = 0;
                if (labelX > maxX)
                    labelX = maxX;

                int labelY = entity.ZInt + 3;
                if (IsSpecialCom(in entity) || (entity.SlotIndex < 0 || entity.SlotIndex >= SlotCount))
                {
                    WriteGlyph(glyphBuffer, ref sequence, 'C', sheetIndex, labelX, labelY, BattleEntityOverlayGlyphType.Label);
                    WriteGlyph(glyphBuffer, ref sequence, 'o', sheetIndex, labelX + GlyphAdvance, labelY, BattleEntityOverlayGlyphType.Label);
                    WriteGlyph(glyphBuffer, ref sequence, 'm', sheetIndex, labelX + GlyphAdvance * 2, labelY, BattleEntityOverlayGlyphType.Label);
                }
                else
                {
                    int offset = 0;
                    if (bracketed)
                    {
                        WriteGlyph(glyphBuffer, ref sequence, '[', sheetIndex, labelX, labelY, BattleEntityOverlayGlyphType.Label);
                        offset = 1;
                    }

                    for (int i = 0; i < labelLength - (bracketed ? 2 : 0); i++)
                        WriteGlyph(glyphBuffer, ref sequence, slotLabelChars[entity.SlotIndex, i], sheetIndex, labelX + (offset + i) * GlyphAdvance, labelY, BattleEntityOverlayGlyphType.Label);

                    if (bracketed)
                        WriteGlyph(glyphBuffer, ref sequence, ']', sheetIndex, labelX + (labelLength - 1) * GlyphAdvance, labelY, BattleEntityOverlayGlyphType.Label);
                }
            }

            glyphCount = sequence;
            return true;
        }

        private static int GetLabelLength(in BattleEntityOverlayRuntimeSlot entity, char[,] slotLabelChars)
        {
            if ((entity.SlotIndex >= 20 && (entity.RelationTeam == 5 || entity.ObjType != 0)) || entity.HitStop <= -25)
                return IsSpecialCom(in entity) ? 3 : 0;

            if (entity.SlotIndex < 0 || entity.SlotIndex >= SlotCount)
                return 3;

            int length = 0;
            while (length < SlotLabelCharacterCapacity && slotLabelChars[entity.SlotIndex, length] != '\0')
                length++;
            return length;
        }

        private static bool IsSpecialCom(in BattleEntityOverlayRuntimeSlot entity)
        {
            return entity.SlotIndex >= 20 &&
                   entity.HitStop > -25 &&
                   entity.ObjType == 0 &&
                   entity.RelationTeam == 5 &&
                   (entity.Oid < 30 || entity.Oid >= 50 || entity.Oid == 38);
        }

        private static int ResolveRelationSheet(int relationTeam)
        {
            return relationTeam >= 1 && relationTeam <= 4 ? relationTeam : 0;
        }

        private static void WriteGlyph(
            BattleEntityOverlayGlyph[] glyphBuffer,
            ref int sequence,
            char charCode,
            int sheetIndex,
            int pixelX,
            int pixelY,
            BattleEntityOverlayGlyphType type)
        {
            glyphBuffer[sequence] = new BattleEntityOverlayGlyph
            {
                CharCode = charCode,
                SheetIndex = sheetIndex,
                PixelX = pixelX,
                PixelY = pixelY,
                Sequence = sequence,
                Type = type,
            };
            sequence++;
        }
    }
}
