using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TestShuffler;

namespace TestShuffler
{
    public class ExtendedActionBlock<TItem> : ITargetBlock<TItem>
    {
        private readonly ActionBlock<TItem> _block;
        private readonly Func<TItem, Task> _actionAsync;

        public Task Completion => _block.Completion;

        public ExtendedActionBlock(Func<TItem, Task> actionAsync, int parallelismDegree, int maxSize)
        {
            _actionAsync = Ensure.NotNull(nameof(actionAsync), actionAsync);

            _block =
                new ActionBlock<TItem>(
                    RunActionAsync,
                    new ExecutionDataflowBlockOptions
                    {
                        BoundedCapacity = maxSize,
                        MaxDegreeOfParallelism = parallelismDegree
                    });
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TItem messageValue, ISourceBlock<TItem>? source, bool consumeToAccept) =>
            ((ITargetBlock<TItem>)_block).OfferMessage(messageHeader, messageValue, source, consumeToAccept);

        public void Complete() => _block.Complete();

        public void Fault(Exception exception) =>
            ((ITargetBlock<TItem>)_block).Fault(exception);

        public bool Post(TItem item) =>
            _block.Post(item);

        private async Task RunActionAsync(TItem item)
        {
            try
            {
                await _actionAsync(item);
            }
            catch (Exception exception)
            {
                ExceptionHandler.Handle(exception);
            }
        }
    }
}