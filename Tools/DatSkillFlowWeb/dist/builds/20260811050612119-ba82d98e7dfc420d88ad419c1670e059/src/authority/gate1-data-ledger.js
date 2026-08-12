// dat-skill-flow-build:20260811050612119-ba82d98e7dfc420d88ad419c1670e059
                                          
               
                    
                   
                   
 

export const gate1DataAuthorityLedger                                     = Object.freeze([
    {
        id: "dat.envelope.absolute-key-offset",
        summary: "Skip the 123-byte envelope and add/subtract the 37-byte key using absolute file offset modulo key length.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\include\\dat_parser.h",
        region: "DAT_KEY/DAT_SKIP/DAT_KEYLEN and dat_decrypt",
    },
    {
        id: "dat.frame.defaults-and-duplicates",
        summary: "Frame wait defaults to 1 and duplicate frame IDs update the lookup index to the last parsed frame.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\include\\dat_parser.h",
        region: "FrameData defaults and CharData::add_frame",
    },
    {
        id: "dat.itr-zwidth-default",
        summary: "ITR zwidth defaults to 15 when absent.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\include\\dat_parser.h",
        region: "ItrData::zwidth",
    },
    {
        id: "dat.cpoint.alias-side-effects",
        summary: "fronthurtact also writes injury and backhurtact also writes cover in parse order.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\data\\dat_parser.cpp",
        region: "parse_cpoint",
    },
    {
        id: "dat.frame.authority-range",
        summary: "Preserve every parsed frame in the CST, but add_frame admits only frame ids in [0, 600) to the runtime authority projection.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\include\\dat_parser.h",
        region: "CharData::MAX_FRAME_ID and CharData::add_frame",
    },
    {
        id: "data.object-allocation-occupies-duplicate",
        summary: "Decrypt failure leaves an OID available to a later entry, while successful alloc_char occupies it before parsing, so even parse failure causes later duplicates to be skipped.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\core\\loading.cpp; J:\\QQFile\\NTSD2.4\\ntsd_cpp\\include\\game_world.h",
        region: "load_oid_impl and GameWorld::alloc_char/has_char",
    },
    {
        id: "sprite.grid-row-is-columns",
        summary: "The DAT row value is passed as SpriteSheet columns; declared ranges select a local picture without a row*col capacity gate, and src_rect advances by w+1 and h+1.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\core\\loading.cpp; J:\\QQFile\\NTSD2.4\\ntsd_cpp\\include\\renderer.h",
        region: "load_oid_impl sprite-range loop and SpriteSheet::src_rect",
    },
    {
        id: "sprite.pic-999-no-render",
        summary: "Picture 999 exits entity rendering before sprite lookup.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\render\\renderer.cpp",
        region: "Renderer::draw_entity fd->pic == 999 guard",
    },
    {
        id: "bmp.loader-validates-pixel-storage",
        summary: "BMP assets are admitted through SDL_LoadBMP; metadata preflight therefore rejects pixel arrays shorter than their padded scanline storage, including BI_BITFIELDS data.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\core\\loading.cpp; J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\render\\renderer.cpp",
        region: "load_bmp_wpath SDL_LoadBMP path and Renderer::load_sprite",
    },
]);
