#include "ntsd28/simulation_tick_driver.h"
#include <cstddef>
#include <iostream>
int main() {
 std::cout << offsetof(ntsd28::SimulationTickResult28, audio_events) << " "
           << sizeof(ntsd28::WorldAudioEvent28) << " "
           << offsetof(ntsd28::WorldAudioEvent28, resource_path) << " "
           << sizeof(ntsd28::SimulationTickResult28);
}
