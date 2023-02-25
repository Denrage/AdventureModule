using Denrage.AdventureModule.Adventure.Services;
using Denrage.AdventureModule.Libs.Messages.Data;
using System;
using System.Collections.Concurrent;

namespace Denrage.AdventureModule.Adventure.Elements
{
    public class DialogBuilder
    {
        private readonly ConcurrentDictionary<Guid, AdventureDialog> dialogs = new ConcurrentDictionary<Guid, AdventureDialog>();
        private readonly SynchronizationService synchronizationService;

        public DialogBuilder(SynchronizationService synchronizationService)
        {
            this.synchronizationService = synchronizationService;

            this.synchronizationService.Register(typeof(DialogState), state =>
            {
                var dialogState = (DialogState)state;
                _ = this.dialogs.TryGetValue(dialogState.Id, out var adventureDialog);
                adventureDialog.SetState(dialogState);
            });
        }

        public DialogGraph Create(string name)
        {
            var graph = new DialogGraph(name);
            graph.OnBuilt += dialog =>
            {
                dialog.StateChanged += state => this.synchronizationService.SendNewState(state, default);
                this.dialogs.TryAdd(dialog.Id, dialog);
            };

            return graph;
        }
    }
}

