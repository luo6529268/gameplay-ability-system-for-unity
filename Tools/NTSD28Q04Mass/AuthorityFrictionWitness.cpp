#include "ntsd28/physics_integrator.h"
#include <iomanip>
#include <iostream>

int main() {
    struct Case { const char* id; double y; int reference; double vx, vy, vz; bool bx, bz; };
    const Case cases[] = {
        {"grounded", 0, 0, 5, 0, -5, false, false},
        {"reverse", 0, 0, -3, 0, 3, false, false},
        {"small", 0, 0, 0.25, 0, -0.25, false, false},
        {"blocked", 0, 0, 5, 0, -5, true, true},
        {"airborne", -5, 0, 5, 0, -5, false, false},
        {"landing", -1, 0, 5, 2, -5, false, false},
        {"negative_floor", -10, -10, 5, 0, -5, false, false},
        {"integer_snapshot", -0.5, 0, 5, 0, -5, false, false}
    };
    std::cout << "id\tx\ty\tz\tvx\tvy\tvz\n" << std::setprecision(17);
    for (const auto& c : cases) {
        ntsd28::PhysicsContext28 context;
        context.object_type = 0;
        context.collision_y_reference = c.reference;
        context.collision.positive_x = c.bx;
        context.collision.negative_depth = c.bz;
        ntsd28::Position28 position{};
        position.y = static_cast<int>(c.y);
        position.precise_y = c.y;
        ntsd28::Motion28 motion{};
        motion.x = c.vx; motion.y = c.vy; motion.z = c.vz;
        const auto result = ntsd28::PhysicsIntegrator28{}.step(context, position, motion);
        (void)result;
        std::cout << c.id << '\t' << position.precise_x << '\t' << position.precise_y
                  << '\t' << position.precise_z << '\t' << motion.x << '\t' << motion.y
                  << '\t' << motion.z << '\n';
    }
}
