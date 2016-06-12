namespace RavuAlHemio.PSD
{
    public enum BlendMode : int
    {
        // "pass"
        PassThrough = 0x70617373,

        // "norm"
        Normal = 0x6e6f726d,

        // "diss"
        Dissolve = 0x64697373,

        // "dark"
        Darken = 0x6461726b,

        // "mul "
        Multiply = 0x6D756C20,

        // "idiv"
        ColorBurn = 0x69646976,

        // "lbrn"
        LinearBurn = 0x6c62726e,

        // "dkCl"
        DarkerColor = 0x646b436c,

        // "lite"
        Lighten = 0x6c697465,

        // "scrn"
        Screen = 0x7363726e,

        // "div "
        ColorDodge = 0x64697620,

        // "lddg"
        LinearDodge = 0x6c646467,

        // "lgCl"
        LighterColor = 0x6c67436c,

        // "over"
        Overlay = 0x6f766572,

        // "sLit"
        SoftLight = 0x734c6974,

        // "hLit"
        HardLight = 0x684c6974,

        // "vLit"
        VividLight = 0x764c6974,

        // "lLit"
        LinearLight = 0x6c4c6974,

        // "pLit"
        PinLight = 0x704c6974,

        // "hMix"
        HardMix = 0x684d6978,

        // "diff"
        Difference = 0x64696666,

        // "smud"
        Exclusion = 0x736d7564,

        // "fsub"
        Subtract = 0x66737562,

        // "fdiv"
        Divide = 0x66646976,

        // "hue "
        Hue = 0x68756520,

        // "sat "
        Saturation = 0x73617420,

        // "colr"
        Color = 0x636f6c72,

        // "lum "
        Luminosity = 0x6c756d20
    }
}
