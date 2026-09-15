#define wmain capture_runner_entry_unused
#include "authority_source_capture_main.cpp"
#undef wmain
#include "ntsd28/dat_parser.h"
#include "ntsd28/simulation_tick_driver.h"

static const char* key_names[] = {"attack","jump","defend","right","left","up","down"};
static const ntsd28::InputKey28 keys[] = {ntsd28::InputKey28::attack,ntsd28::InputKey28::jump,ntsd28::InputKey28::defend,
    ntsd28::InputKey28::right,ntsd28::InputKey28::left,ntsd28::InputKey28::depth_up,ntsd28::InputKey28::depth_down};
static const char* fields[] = {"aaction","taction","daction","uzaction","dzaction","faction","baction","jaction"};
struct SelectionCase {
    const char* group = "binding";
    int selector = 0, variant = 0, victim_action = 131;
    bool facing = false, victim_declared = true, selected_cpoint = true;
    std::array<int,8> actions{};
    std::array<int,7> current{}, previous{}, edge{};
    std::vector<int> declared;
};
static void activate(SelectionCase& c, int selector) {
    if (selector == 0 || selector == 1) { c.current[0] = 1; c.edge[0] = 5; }
    if (selector == 1) c.current[3] = 1;
    if (selector == 2) { c.current[2] = 1; c.edge[2] = 5; }
    if (selector == 3) { c.previous[5] = 1; c.edge[5] = 5; }
    if (selector == 4) { c.previous[6] = 1; c.edge[6] = 5; }
    if (selector == 5 || selector == 6) {
        int key = selector == 5 ? (c.facing ? 3 : 4) : (c.facing ? 4 : 3);
        c.previous[key] = 1; c.edge[key] = 5;
    }
    if (selector == 7) { c.current[1] = 1; c.edge[1] = 5; }
}
static void write_keys(const std::array<int,7>& values) {
    std::cout << '{';
    for (int i=0;i<7;++i) { if(i)std::cout<<',';std::cout<<'"'<<key_names[i]<<"\":"<<values[i]; }
    std::cout << '}';
}
static void write_selection_entity(const ntsd28::EntityState28* e) {
    if (!e) {std::cout<<"null";return;}
    const auto* f=e->definition->frame(e->frame.action);
    const auto* snapshot=e->definition->frame(e->frame.tick_action_snapshot);
    std::cout<<"{\"raw\":";write_entity(std::cout,*e,1);
    std::cout<<",\"available\":"<<(f?"true":"false")<<",\"state\":"<<(f?f->values.integer("state").value_or(0):0)
        <<",\"wait\":"<<(f?f->values.integer("wait").value_or(0):0)<<",\"next\":"<<(f?f->values.integer("next").value_or(0):0)
        <<",\"snapshotAvailable\":"<<(snapshot?"true":"false")<<",\"snapshotState\":"<<(snapshot?snapshot->values.integer("state").value_or(0):0)
        <<",\"catchTarget\":"<<e->catch_target_slot_8c<<",\"catchSource\":"<<e->catch_source_slot_90<<",\"timeout\":"<<e->catch_timeout_94
        <<",\"link\":"<<e->interaction_state<<",\"parent\":"<<e->linked_parent_slot<<",\"child\":"<<e->linked_child_slot
        <<",\"input\":";write_b2_input_entity(std::cout,*e,1);std::cout<<'}';
}
static void write_selection_state(ntsd28::BattleWorld28& w) {
    std::cout<<"{\"entities\":[";write_selection_entity(w.entity(0));std::cout<<',';write_selection_entity(w.entity(1));
    std::cout<<"],\"random\":";write_b2_initial_random(std::cout,w.random().state(),w.random().synchronized_table_hash());std::cout<<'}';
}
static void write_selection_calls() {
    std::cout<<"{\"crt\":";write_b2_crt_calls(std::cout);std::cout<<",\"synchronized\":";write_b2_synchronized_calls(std::cout);std::cout<<'}';
}
static void write_selection_pass(const ntsd28::WorldCatchRelationPass28& r) {
    std::cout<<"{\"success\":"<<(r.success?"true":"false")<<",\"active\":"<<r.active_relations
        <<",\"timeoutChanges\":"<<r.timeout_changes<<",\"released\":"<<r.released_relations<<",\"thrown\":"<<r.thrown_relations
        <<",\"armedThrowInjuries\":"<<r.armed_throw_injuries<<",\"deferredThrowInjuries\":"<<r.deferred_throw_injuries
        <<",\"transitions\":"<<r.input_action_transitions<<",\"directionUpdates\":"<<r.direction_control_updates
        <<",\"broken\":"<<r.broken_relations<<",\"diagnostics\":[";
    bool first=true;for(const auto& d:r.diagnostics){if(!first)std::cout<<',';first=false;std::cout<<'"'<<json_escape(d)<<'"';}std::cout<<"]}";
}
static void emit_selection(int index,const SelectionCase& c) {
    std::ostringstream catcher_dat,victim_dat;
    catcher_dat<<"<bmp_begin>\nname: CatchInputSelector\n<bmp_end>\n<frame> 100 initial\nstate: 9 wait: 100 next: 100 centerx: 39 centery: 79\n"
        <<"cpoint:\nkind: 1 x: 50 y: 60 vaction: 130 throwvx: 0 decrease: 0";
    for(int i=0;i<8;++i)catcher_dat<<' '<<fields[i]<<": "<<c.actions[i];
    catcher_dat<<"\ncpoint_end:\n<frame_end>\n";
    for(int action:c.declared){
        catcher_dat<<"<frame> "<<action<<" selected\nstate: 9 wait: 23 next: 0 centerx: 399 centery: 799\n";
        if(c.selected_cpoint)catcher_dat<<"cpoint:\nkind: 1 vaction: "<<c.victim_action<<"\ncpoint_end:\n";
        catcher_dat<<"<frame_end>\n";
    }
    victim_dat<<"<bmp_begin>\nname: CatchInputVictim\n<bmp_end>\n<frame> 130 initial\nstate: 10 wait: 83 next: 130 centerx: 35 centery: 70\n"
        <<"cpoint:\nkind: 2\ncpoint_end:\n<frame_end>\n";
    if(c.victim_declared)victim_dat<<"<frame> "<<c.victim_action<<" selected\nstate: 10 wait: 31 next: 0 centerx: 17 centery: 27\ncpoint:\nkind: 2\ncpoint_end:\n<frame_end>\n";
    const auto a=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(catcher_dat.str()));
    const auto b=std::make_shared<const ntsd28::DatDocument>(ntsd28::DatParser{}.parse_text(victim_dat.str()));
    if(!a->ok()||!b->ok())throw std::runtime_error("selector DAT rejected");
    ntsd28::BattleWorld28 world;world.random().reset_from_seed(42);
    ntsd28::ObjectDefinitionCatalog28 catalog;catalog.upsert_definition(77,0,"selector-catcher.dat",a);catalog.upsert_definition(78,0,"selector-victim.dat",b);
    for(int slot:{0,1}){
        ntsd28::SpawnRequest28 spawn;spawn.object_id=77+slot;spawn.object_type=0;spawn.definition=slot?b:a;
        spawn.initial_action=slot?130:100;spawn.hp=500;spawn.mp=500;
        spawn.position.x=slot?350:300;spawn.position.y=0;spawn.position.z=250;
        if(!world.spawn_at(slot,spawn).success)throw std::runtime_error("selector spawn failed");
        auto& e=*world.entity(slot);e.frame.frame_counter=7+slot;e.frame.action_latch=11+slot;e.frame.previous_action_078=slot?130:100;
        e.frame.facing=slot?!c.facing:c.facing;
    }
    auto& catcher=*world.entity(0);auto& victim=*world.entity(1);
    catcher.catch_target_slot_8c=1;catcher.catch_timeout_94=300;victim.catch_source_slot_90=0;
    for(int i=0;i<7;++i){catcher.input.current.set(keys[i],c.current[i]!=0);catcher.input.previous.set(keys[i],c.previous[i]!=0);
        catcher.input.pending.set(keys[i],c.current[i]!=0);catcher.input.edge_window[static_cast<std::size_t>(keys[i])]=static_cast<unsigned char>(c.edge[i]);}
    world.snapshot_actions();
    const ntsd28_playable::BattleConfig28 defaults;
    std::cout<<"{\"index\":"<<index<<",\"params\":{\"group\":\""<<c.group<<"\",\"selector\":"<<c.selector<<",\"variant\":"<<c.variant
        <<",\"facing\":"<<(c.facing?"true":"false")<<",\"victimAction\":"<<c.victim_action<<",\"victimDeclared\":"<<(c.victim_declared?"true":"false")
        <<",\"selectedCpoint\":"<<(c.selected_cpoint?"true":"false")<<",\"actions\":[";
    for(int i=0;i<8;++i){if(i)std::cout<<',';std::cout<<c.actions[i];}std::cout<<"],\"declared\":[";
    for(std::size_t i=0;i<c.declared.size();++i){if(i)std::cout<<',';std::cout<<c.declared[i];}
    std::cout<<"],\"current\":";write_keys(c.current);std::cout<<",\"previous\":";write_keys(c.previous);std::cout<<",\"edge\":";write_keys(c.edge);
    std::cout<<",\"seed\":42},\"catcherDat\":\""<<json_escape(catcher_dat.str())<<"\",\"victimDat\":\""<<json_escape(victim_dat.str())
        <<"\",\"config\":{\"stageWidth\":800,\"stageNear\":180,\"stageFar\":350,\"hpGate\":"<<defaults.selected_mode_default_hp_regen_gate_28
        <<",\"mpGate\":"<<defaults.selected_mode_mp_regen_gate_2c<<",\"dropGate\":"<<defaults.selected_mode_drop_gate_4c<<"},\"before\":";
    write_selection_state(world);direct_crt_calls.clear();direct_synchronized_calls.clear();
    const auto pass=world.advance_catch_relations();std::cout<<",\"pass\":";write_selection_pass(pass);std::cout<<",\"after\":";write_selection_state(world);
    std::cout<<",\"calls\":";write_selection_calls();
    ntsd28::SimulationTickOptions28 options;options.stage_bounds=ntsd28::StageBounds28{800,180,350};options.controls.resize(ntsd28::EngineProfile28::maximum_slots);
    options.resource_rules.selected_mode_default_hp_regen_gate_28=defaults.selected_mode_default_hp_regen_gate_28;
    options.resource_rules.selected_mode_mp_regen_gate_2c=defaults.selected_mode_mp_regen_gate_2c;options.selected_mode_weapon_drop_4c=defaults.selected_mode_drop_gate_4c;
    direct_crt_calls.clear();direct_synchronized_calls.clear();const auto tick=ntsd28::SimulationTickDriver28{}.step(world,catalog,options);
    std::cout<<",\"following\":";write_selection_state(world);std::cout<<",\"followingCalls\":";write_selection_calls();
    std::cout<<",\"followingPass\":";write_selection_pass(tick.catch_relations);std::cout<<",\"lifecycleSuccess\":"<<(tick.lifecycle.success?"true":"false")<<"}\n";
}
int wmain(int,wchar_t**) {
    try{
        std::cout<<std::setprecision(std::numeric_limits<double>::max_digits10);int index=0;
        for(int selector=0;selector<8;++selector)for(bool facing:{false,true})
        for(int target:{0,99,-99,900,-900,999,-999,1000,-1000}){
            SelectionCase c;c.selector=selector;c.facing=facing;c.actions[selector]=target;activate(c,selector);emit_selection(index++,c);
            if(target!=0 && std::abs(target)<=999){c.declared.push_back(std::abs(target));emit_selection(index++,c);}
        }
        for(bool facing:{false,true})for(int last=0;last<8;++last)for(bool cancel:{false,true}){
            SelectionCase c;c.group="priority";c.selector=last;c.variant=cancel?1:0;c.facing=facing;
            for(int i=0;i<=last;++i){c.actions[i]=201+i;c.declared.push_back(201+i);activate(c,i);}
            if(cancel)c.actions[last]=0;emit_selection(index++,c);
        }
        for(int selector=0;selector<8;++selector)for(bool facing:{false,true})for(int variant=0;variant<5;++variant){
            SelectionCase c;c.group="sample_gate";c.selector=selector;c.variant=variant;c.facing=facing;c.actions[selector]=900;c.declared={900};activate(c,selector);
            if(variant==0)std::swap(c.current,c.previous);
            if(variant==1){c.current.fill(0);c.previous.fill(0);}
            if(variant>=2)for(auto& edge:c.edge)if(edge)edge=variant==2?0:variant==3?1:255;
            emit_selection(index++,c);
        }
        for(bool facing:{false,true}){
            for(int vaction:{0,131,-131,999,1000}){
                SelectionCase c;c.group="victim_binding";c.facing=facing;c.actions[0]=900;c.declared={900};c.victim_action=vaction;c.victim_declared=false;activate(c,0);emit_selection(index++,c);
                if(vaction>=0 && vaction<=999){c.victim_declared=true;emit_selection(index++,c);}
            }
            SelectionCase c;c.group="victim_binding";c.facing=facing;c.actions[0]=900;c.declared={900};c.selected_cpoint=false;activate(c,0);emit_selection(index++,c);
        }
        if(index!=370)throw std::runtime_error("selector case count changed");
        return 0;
    }catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 91;}
}
