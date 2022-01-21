namespace API.Services {
    public interface IMessageService: IDisposable {
        public void BeginTransaction();
        public void CommitTransaction();
        public void RollbackTransaction();
        public void Send(string message);
        // Will eventually add Complete, Deadletter, Defer, and Receive
    }
}
