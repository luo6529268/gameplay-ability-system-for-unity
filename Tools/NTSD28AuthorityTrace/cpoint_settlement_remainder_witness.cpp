#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"
static bool following_mode=false;

static void state(ntsd28::BattleWorld28& world) {
    std::cout << '[';
    for (int slot=0;slot<2;++slot) {
        if(slot)std::cout<<',';
        const auto* e=world.entity(slot);
        const auto* f=e->definition->frame(e->frame.action);
        std::cout<<"{\"raw\":";write_entity(std::cout,*e,1);
        std::cout<<",\"available\":"<<(f?"true":"false")
            <<",\"wait\":"<<(f?f->values.integer("wait").value_or(0):0)
            <<",\"next\":"<<(f?f->values.integer("next").value_or(0):0)
            <<",\"target\":"<<e->catch_target_slot_8c<<",\"source\":"<<e->catch_source_slot_90
            <<",\"timeout\":"<<e->catch_timeout_94<<'}';
    }
    std::cout<<']';
}
static void emit(int index,int mode,int action,bool declared) {
    if(following_mode && index!=5 && index!=9 && index!=22)return;
    std::ostringstream a,b;
    a<<"<bmp_begin>\nname: RemainderA\n<bmp_end>\n<frame> 100 initial\nstate: 9 wait: 43 next: 100 centerx: 39 centery: 79\n"
      <<"cpoint:\nkind: 1 x: 50 y: 60 vaction: "<<action<<" hurtable: 0 injury: 0 decrease: "<<(mode==3?-1:0)<<"\ncpoint_end:\n<frame_end>\n";
    b<<"<bmp_begin>\nname: RemainderB\n<bmp_end>\n<frame> 130 initial\nstate: 10 wait: 83 next: 130 centerx: 35 centery: 70\n"
      <<"cpoint:\nkind: 2 x: 17 y: 27\ncpoint_end:\n<frame_end>\n";
    int target=action<0?-action:action;
    if(declared) {
        if(mode==0)b<<"<frame> "<<target<<" target\nstate: 10 wait: 31 next: 0 centerx: 17 centery: 27\ncpoint:\nkind: 2 x: 3 y: 5\ncpoint_end:\n<frame_end>\n";
        else {
            a<<"<frame> 0 target\nstate: 0 wait: 23 next: 0\n<frame_end>\n";
            b<<"<frame> 181 target\nstate: 12 wait: 31 next: 0\n<frame_end>\n";
        }
    }
    auto ad=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(a.str()));
    auto bd=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(b.str()));
    if(!ad->ok()||!bd->ok())throw std::runtime_error("DAT rejected");
    ntsd28::BattleWorld28 world;world.random().reset_from_seed(42);
    for(int slot=0;slot<2;++slot) {
        ntsd28::SpawnRequest28 req;req.object_id=77+slot;req.object_type=0;req.definition=slot?bd:ad;
        req.initial_action=slot?130:100;req.hp=500;req.mp=500;
        req.position.x=slot?350:300;req.position.y=0;req.position.z=250;
        if(!world.spawn_at(slot,req).success)throw std::runtime_error("spawn failed");
        auto& e=*world.entity(slot);e.frame.frame_counter=7+slot;e.frame.action_latch=11+slot;e.frame.previous_action_078=slot?130:100;
    }
    world.entity(0)->catch_target_slot_8c=mode==1?-1:1;
    world.entity(0)->catch_timeout_94=mode==3?0:300;
    world.entity(1)->catch_source_slot_90=mode==2?-1:0;
    world.snapshot_actions();
    std::cout<<"{\"index\":"<<index<<",\"mode\":"<<mode<<",\"action\":"<<action<<",\"declared\":"<<(declared?"true":"false")
      <<",\"catcherDat\":\""<<json_escape(a.str())<<"\",\"victimDat\":\""<<json_escape(b.str())<<"\",\"before\":";
    state(world);
    bool success;
    if(mode==0)success=world.settle_catch_relations().success;
    else success=world.advance_catch_relations().success;
    std::cout<<",\"success\":"<<(success?"true":"false")<<",\"after\":";state(world);
    if(following_mode) {
        ntsd28::ObjectDefinitionCatalog28 catalog;
        catalog.upsert_definition(77,0,"catcher.dat",ad);catalog.upsert_definition(78,0,"victim.dat",bd);
        ntsd28::SimulationTickOptions28 options;
        options.stage_bounds=ntsd28::StageBounds28{800,180,350};
        options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
        const ntsd28_playable::BattleConfig28 defaults;
        options.resource_rules.selected_mode_default_hp_regen_gate_28=defaults.selected_mode_default_hp_regen_gate_28;
        options.resource_rules.selected_mode_mp_regen_gate_2c=defaults.selected_mode_mp_regen_gate_2c;
        options.selected_mode_weapon_drop_4c=defaults.selected_mode_drop_gate_4c;
        const auto result=ntsd28::SimulationTickDriver28{}.step(world,catalog,options);
        std::cout<<",\"following\":";state(world);
        std::cout<<",\"lifecycleSuccess\":"<<(result.lifecycle.success?"true":"false");
    }
    std::cout<<"}\n";
}
int wmain(int argc,wchar_t**) {
    following_mode=argc>1;
    try {
        std::cout<<std::setprecision(std::numeric_limits<double>::max_digits10);int index=0;
        for(int action:{0,181,857,900,-900,998,999,-999,1000,-1000}) {
            emit(index++,0,action,false);
            if(std::abs(action)<1000)emit(index++,0,action,true);
        }
        for(int mode:{1,2,3})for(bool declared:{false,true})emit(index++,mode,0,declared);
        if(index!=24)throw std::runtime_error("case count");
        return 0;
    } catch(const std::exception& e) {std::cerr<<e.what()<<'\n';return 2;}
}
