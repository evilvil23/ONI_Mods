using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace sinevil.Robot_Animal_Remastered
{
    [SerializationConfig((MemberSerialization)1)]
    public class MyAirFilter : StateMachineComponent<MyAirFilter.StatesInstance>, IGameObjectEffectDescriptor
    {
        public bool HasFilter()
        {
            return true;
        }

        public bool IsConvertable()
        {
            return this.elementConverter.HasEnoughMassToStartConverting(false);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            base.smi.StartSM();
        }

        public List<Descriptor> GetDescriptors(GameObject go)
        {
            return null;
        }

        [MyCmpGet]
        private Operational operational;

        [MyCmpGet]
        private Storage storage;

        [MyCmpGet]
        private ElementConverter elementConverter;

        [MyCmpGet]
        private ElementConsumer elementConsumer;

        public class StatesInstance : GameStateMachine<MyAirFilter.States, MyAirFilter.StatesInstance, MyAirFilter, object>.GameInstance
        {
            public StatesInstance(MyAirFilter smi) : base(smi)
            {
            }
        }

        public class States : GameStateMachine<MyAirFilter.States, MyAirFilter.StatesInstance, MyAirFilter>
        {
            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = this.waiting;
                this.waiting.EventTransition(GameHashes.OnStorageChange, this.hasFilter, (MyAirFilter.StatesInstance smi) => smi.master.HasFilter() && smi.master.operational.IsOperational).EventTransition(GameHashes.OperationalChanged, this.hasFilter, (MyAirFilter.StatesInstance smi) => smi.master.HasFilter() && smi.master.operational.IsOperational);
                this.hasFilter.EventTransition(GameHashes.OperationalChanged, this.waiting, (MyAirFilter.StatesInstance smi) => !smi.master.operational.IsOperational).Enter("EnableConsumption", delegate (MyAirFilter.StatesInstance smi)
                {
                    smi.master.elementConsumer.EnableConsumption(true);
                }).Exit("DisableConsumption", delegate (MyAirFilter.StatesInstance smi)
                {
                    smi.master.elementConsumer.EnableConsumption(false);
                }).DefaultState(this.hasFilter.idle);
                this.hasFilter.idle.EventTransition(GameHashes.OnStorageChange, this.hasFilter.converting, (MyAirFilter.StatesInstance smi) => smi.master.IsConvertable());
                this.hasFilter.converting.Enter("SetActive(true)", delegate (MyAirFilter.StatesInstance smi)
                {
                    smi.master.operational.SetActive(true, false);
                }).Exit("SetActive(false)", delegate (MyAirFilter.StatesInstance smi)
                {
                    smi.master.operational.SetActive(false, false);
                }).EventTransition(GameHashes.OnStorageChange, this.hasFilter.idle, (MyAirFilter.StatesInstance smi) => !smi.master.IsConvertable());
            }

            public MyAirFilter.States.ReadyStates hasFilter;

            public GameStateMachine<MyAirFilter.States, MyAirFilter.StatesInstance, MyAirFilter, object>.State waiting;

            public class ReadyStates : GameStateMachine<MyAirFilter.States, MyAirFilter.StatesInstance, MyAirFilter, object>.State
            {
                public GameStateMachine<MyAirFilter.States, MyAirFilter.StatesInstance, MyAirFilter, object>.State idle;

                public GameStateMachine<MyAirFilter.States, MyAirFilter.StatesInstance, MyAirFilter, object>.State converting;
            }
        }
    }
}
