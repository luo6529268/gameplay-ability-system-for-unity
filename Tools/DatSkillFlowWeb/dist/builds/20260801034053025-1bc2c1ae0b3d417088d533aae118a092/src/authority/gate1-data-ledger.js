// dat-skill-flow-build:20260801034053025-1bc2c1ae0b3d417088d533aae118a092
                                          
               
                    
                   
                   
 

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
        id: "dat.bdy-zwidth-height-compatibility",
        summary: "Expose the verified NTSD compatibility projection where bdy zwidth aliases the runtime height slot without rewriting source bytes.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\data\\dat_parser.cpp",
        region: "parse_bdy compatibility contract",
    },
    {
        id: "data.object-first-successful-duplicate",
        summary: "Object entries are attempted in source order and an already loaded OID prevents later duplicates from replacing the first successful load.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\core\\loading.cpp",
        region: "LoadingScene::parse_data_txt, run_impl, and load_oid_impl",
    },
    {
        id: "sprite.grid-row-is-columns",
        summary: "The DAT row value is passed as SpriteSheet columns; source cells advance by w+1 and h+1.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\core\\loading.cpp; J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\render\\renderer.cpp",
        region: "load_oid_impl and Renderer::load_sprite/src_rect",
    },
    {
        id: "sprite.pic-999-no-render",
        summary: "Picture 999 exits entity rendering before sprite lookup.",
        source: "J:\\QQFile\\NTSD2.4\\ntsd_cpp\\src\\render\\renderer.cpp",
        region: "Renderer entity draw path before sprite range lookup",
    },
]);
