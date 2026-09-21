#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/fusion_catalog.h"
#include "ntsd28/simulation_tick_driver.h"

static void write_fusion_entity(const ntsd28::EntityState28* e) {
    if (!e) { std::cout << "null"; return; }
    const auto* f = e->definition->frame(e->frame.action);
    const auto* snapshot = e->definition->frame(e->frame.tick_action_snapshot);
    std::cout << "{\"raw\":"; write_entity(std::cout,*e,1);
    std::cout << ",\"available\":" << (f ? "true" : "false")
        << ",\"state\":" << (f ? f->values.integer("state").value_or(0) : 0)
        << ",\"wait\":" << (f ? f->values.integer("wait").value_or(0) : 0)
        << ",\"next\":" << (f ? f->values.integer("next").value_or(0) : 0)
        << ",\"snapshotAvailable\":" << (snapshot ? "true" : "false")
        << ",\"snapshotState\":" << (snapshot ? snapshot->values.integer("state").value_or(0) : 0)
        << ",\"previousX\":" << e->position.previous_x << ",\"previousY\":" << e->position.previous_y << ",\"previousZ\":" << e->position.previous_z
        << ",\"environment\":" << e->environment_state_320 << ",\"environmentSource\":" << e->environment_source_slot_160
        << ",\"catchSource\":" << e->catch_source_slot_90 << ",\"impactSource\":" << e->impact_source_slot_164
        << ",\"pendingCount\":" << e->pending_hit_impulse.contribution_count << ",\"pendingX\":" << e->pending_hit_impulse.total.x << ",\"pendingY\":" << e->pending_hit_impulse.total.y
        << ",\"pendingZ\":" << e->pending_hit_impulse.total.z
        << ",\"fusionTimer\":" << e->input_special_timer_338 << ",\"fusionDisplayTimer\":" << e->fusion_display_timer_190
        << ",\"fusionPartnerSlot\":" << e->fusion_partner_slot_32c << ",\"fusionPrimaryId\":" << e->fusion_primary_definition_id_330
        << ",\"fusionPartnerId\":" << e->fusion_partner_definition_id_334 << ",\"gate328\":" << e->input_special_gate_328
        << ",\"gate194\":" << e->input_special_gate_194 << ",\"opointLatch\":" << e->opoint_action_latch << ",\"soundLatch\":" << e->sound_action_latch
        << ",\"aiProfile\":" << e->ai_profile_object_id << ",\"dropMode\":" << e->definition_drop_mode
        << ",\"incomingScale\":" << e->incoming_damage_scale_340 << ",\"modeScale\":" << e->mode_damage_scale_percent
        << ",\"reviveVisual318\":" << e->revive_visual_runtime_318
        << ",\"hpConsumed\":" << e->input_hp_consumed_total << ",\"mpConsumed\":" << e->input_mp_consumed_total
        << ",\"score\":" << e->input_score_total_348
        << ",\"input\":"; write_b2_input_entity(std::cout,*e,1); std::cout << '}';
}
static void write_fusion_state(ntsd28::BattleWorld28& world) {
    std::cout << "{\"entities\":["; bool first = true;
    for (int slot : {0,1}) { if (!first) std::cout << ','; first = false; write_fusion_entity(world.entity(slot)); }
    std::cout << "],\"random\":";write_b2_initial_random(std::cout,world.random().state(),world.random().synchronized_table_hash());std::cout << '}';
}
static void write_fusion_calls() {
    std::cout << "{\"crt\":";write_b2_crt_calls(std::cout);std::cout << ",\"synchronized\":";write_b2_synchronized_calls(std::cout);std::cout << '}';
}



static void write_pass(const ntsd28::WorldFusionPass28& p) {
    std::cout << "{\"success\":" << (p.success?"true":"false") << ",\"sourceAvailable\":" << (p.source_available?"true":"false")
        << ",\"fused\":" << p.fused << ",\"defused\":" << p.defused << ",\"unresolved\":" << p.unresolved << ",\"events\":[";
    bool first=true;
    for(const auto& e:p.events) {
        if(!first)std::cout<<',';first=false;
        std::cout << "{\"status\":" << static_cast<int>(e.status) << ",\"primarySlot\":" << e.primary_slot << ",\"partnerSlot\":" << e.partner_slot
            << ",\"primaryOid\":" << e.primary_object_id << ",\"partnerOid\":" << e.partner_object_id << ",\"fusedOid\":" << e.fused_object_id
            << ",\"message\":\"" << json_escape(e.message) << "\"}";
    }
    std::cout<<"]}";
}
static std::string definition_text(int role, int action, bool implicit_action = false) {
    std::ostringstream d;
    d << "<bmp_begin>\nname: FusionWitness" << role << "\nuse_ai: " << 31+role << " drop: " << role+1 << " weapon_hp: " << 21+role << "\n<bmp_end>\n"
      << "<stats> max_mp: " << 600+role*100 << " defend: " << 110+role*20 << " <stats_end>\n";
    if(!implicit_action) d << "<frame> " << action << " current\nstate: " << (role==2?0:2) << " wait: 41 next: " << action << "\n<frame_end>\n";
    if(role!=2)d << "<frame> 112 split\nstate: 0 wait: 41 next: 112\n<frame_end>\n";
    return d.str();
}
static void emit_fusion(int index,const ntsd28::FusionCatalog28& fusions) {
    const auto& record=fusions.records()[index==1?1:0];
    std::array<std::string,3> dat={definition_text(0,20),definition_text(1,10),definition_text(2,record.action,index==1)};
    std::array<std::shared_ptr<const ntsd28::DatDocument>,3> defs;
    ntsd28::ObjectDefinitionCatalog28 catalog, missing_partner;
    const int ids[]={record.id1,record.id2,record.id3};
    for(int i=0;i<3;++i) {
        defs[i]=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(dat[i]));
        if(!defs[i]->ok())throw std::runtime_error("fusion DAT parse failed");
        catalog.upsert_definition(ids[i],0,"fusion-witness.dat",defs[i]);
        if(i!=1)missing_partner.upsert_definition(ids[i],0,"fusion-witness.dat",defs[i]);
    }
    ntsd28::BattleWorld28 world;world.random().reset_from_seed(42);
    for(int slot:{0,1}) {
        ntsd28::SpawnRequest28 r;r.object_id=ids[slot];r.object_type=0;r.definition=defs[slot];r.initial_action=slot==0?20:10;
        r.hp=400;r.mp=497;r.battle_group=3;r.position.x=slot==0?340:300;r.position.z=250;
        if(!world.spawn_at(slot,r).success)throw std::runtime_error("fusion spawn failed");
        auto& e=*world.entity(slot);
        e.current_hp=index==1?(slot==0?200:300):(slot==0?100:120);
        if(index==2 && slot==0)e.current_hp=177;
        e.effective_max_hp=slot==0?250:275;e.base_max_hp=400;
        e.frame.frame_counter=slot==0?7:8;e.frame.action_latch=slot==0?13:11;e.frame.previous_action_078=slot==0?14:12;
        e.position.precise_x=e.position.x+.25;e.position.precise_z=250.5;
        e.motion.x=1.25+slot;e.motion.y=-2.5-slot;e.motion.z=.75+slot;
        e.pending_hit_impulse.total={13.0+slot,17.0+slot,19.0+slot};e.pending_hit_impulse.contribution_count=1;
        e.input_special_timer_338=index==1?17+slot:0;e.fusion_display_timer_190=23+slot;
        e.opoint_action_latch=31+slot;e.sound_action_latch=41+slot;e.revive_visual_runtime_318=51+slot;
        e.mode_damage_scale_percent=125+slot*25;
        e.input_hp_consumed_total=61+slot;e.input_mp_consumed_total=71+slot;e.input_score_total_348=81+slot;
    }
    world.snapshot_actions();
    ntsd28::FusionSystemRules28 rules;
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout << "{\"index\":" << index << ",\"params\":{\"recordIndex\":" << (index==1?1:0)
        << ",\"hpBoundaryReject\":" << (index==2?"true":"false") << ",\"missingPartnerDefinition\":" << (index==3?"true":"false")
        << ",\"firstFeatureGate\":false,\"secondFeatureGate\":false,\"seed\":42}"
        << ",\"config\":{\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"hpGate\":" << defaults.selected_mode_default_hp_regen_gate_28
        << ",\"mpGate\":" << defaults.selected_mode_mp_regen_gate_2c << ",\"dropGate\":" << defaults.selected_mode_drop_gate_4c << "}"
        << ",\"catalogRecord\":{\"id1\":" << record.id1 << ",\"id2\":" << record.id2 << ",\"id3\":" << record.id3
        << ",\"hp\":" << record.hp << ",\"mp\":" << record.mp << ",\"respond\":" << record.respond << ",\"decrease\":" << record.decrease
        << ",\"wait\":" << record.wait << ",\"state\":" << record.state << ",\"action\":" << record.action << ",\"frame\":" << record.frame
        << ",\"chp\":" << record.chp << ",\"hitJa\":" << record.hit_ja << ",\"cover\":" << record.cover
        << ",\"frontHurtAction\":" << record.front_hurt_action << ",\"backHurtAction\":" << record.back_hurt_action << "},\"definitions\":[";
    for(int i=0;i<3;++i){if(i)std::cout<<',';std::cout<<"{\"oid\":"<<ids[i]<<",\"type\":0,\"dat\":\""<<json_escape(dat[i])<<"\"}";}
    std::cout << "],\"beforeMerge\":";write_fusion_state(world);direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto merge=world.advance_native_fusions(fusions,catalog,rules);
    std::cout << ",\"mergeResult\":";write_pass(merge);std::cout<<",\"afterMerge\":";write_fusion_state(world);
    std::cout<<",\"mergeCalls\":";write_fusion_calls();
    if(index==2) {
        if(merge.fused!=0)throw std::runtime_error("fusion strict HP boundary unexpectedly merged");
        std::cout<<",\"timerIntervention\":null,\"beforeDefuse\":null,\"defuseResult\":null,\"afterDefuse\":null,\"following\":null}\n";return;
    }
    if(merge.fused!=1 || world.entity(1)!=nullptr)throw std::runtime_error("fusion merge precondition failed");
    // Test-only branch boundary intervention; this is not elapsed-timer evidence.
    world.entity(0)->input_special_timer_338=0;
    std::cout<<",\"timerIntervention\":{\"slot\":0,\"field\":\"input_special_timer_338\",\"value\":0},\"beforeDefuse\":";
    write_fusion_state(world);direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto split=world.advance_native_fusions(fusions,index==3?missing_partner:catalog,rules);
    std::cout<<",\"defuseResult\":";write_pass(split);std::cout<<",\"afterDefuse\":";write_fusion_state(world);std::cout<<",\"defuseCalls\":";write_fusion_calls();
    if(index==3) {std::cout<<",\"following\":null}\n";return;}
    if(split.defused!=1)throw std::runtime_error("fusion defuse precondition failed");
    ntsd28::SimulationTickOptions28 options;options.fusion_catalog=&fusions;options.fusion_rules=rules;
    options.stage_bounds=ntsd28::StageBounds28{800,180,350};options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28=defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c=defaults.selected_mode_mp_regen_gate_2c;options.selected_mode_weapon_drop_4c=defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto tick=ntsd28::SimulationTickDriver28{}.step(world,catalog,options);
    std::cout<<",\"following\":";write_fusion_state(world);std::cout<<",\"followingCalls\":";write_fusion_calls();
    std::cout<<",\"followingFusionResult\":";write_pass(tick.fusions);std::cout<<",\"lifecycleSuccess\":"<<(tick.lifecycle.success?"true":"false")<<"}\n";
}
int wmain() {
    try {
        std::cout<<std::setprecision(std::numeric_limits<double>::max_digits10);
        const auto fusions=ntsd28::FusionCatalog28::load_file(std::filesystem::path(LR"(J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime\decoded_dat\data\fusion.dat)"));
        if(!fusions.source_available() || !fusions.ok() || fusions.records().size()!=2)throw std::runtime_error("formal fusion catalog unavailable");
        const auto& a=fusions.records()[0];const auto& b=fusions.records()[1];
        if(a.id1!=7||a.id2!=8||a.id3!=51||a.action!=290||a.frame!=112||b.id1!=10||b.id2!=11||b.id3!=52||b.action!=310||b.frame!=112)
            throw std::runtime_error("formal fusion catalog record identity mismatch");
        for(int i=0;i<4;++i)emit_fusion(i,fusions);
        return 0;
    } catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 91;}
}
