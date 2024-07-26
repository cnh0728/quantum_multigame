using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantum.Platformer
{
    unsafe class PlayerSpawnSystem : SystemSignalsOnly, ISignalOnPlayerDataSet
    {
        public void OnPlayerDataSet(Frame frame, PlayerRef player)
        {
            var data = frame.GetPlayerData(player);

            // resolve the reference to the prototpye.
            var prototype = frame.FindAsset<EntityPrototype>(data.CharacterPrototype.Id);

            // Create a new entity for the player based on the prototype.
            var entity = frame.Create(prototype);

            // Create a PlayerLink component. Initialize it with the player. Add the component to the player entity.
            var playerLink = new PlayerLink()
            {
                Player = player,
            };
            frame.Add(entity, playerLink);

            // Offset the instantiated object in the world, based in its ID.
            if (frame.Unsafe.TryGetPointer<Transform3D>(entity, out var transform))
            {
                transform->Position.X = 0 + player;
            }
        }
    }

    public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public CharacterController3D* CharacterController;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            // gets the input for player 0
            Input input = default;

            if(f.Unsafe.TryGetPointer(filter.Entity, out PlayerLink* playerLink)) //접속된 플레이어로 부터 값 가져오기
            {
                input = *f.GetPlayerInput(playerLink->Player);
            }

            if (input.Jump.WasPressed)
            {
                filter.CharacterController->Jump(f);
            }

            filter.CharacterController->Move(f, filter.Entity, input.Direction.XOY);
        }
    }
}
