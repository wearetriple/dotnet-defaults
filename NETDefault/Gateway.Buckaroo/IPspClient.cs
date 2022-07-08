namespace Gateway.Buckaroo;

public interface IPspClient
{
    public Task<TResponse> PostTransactionAsync<TRequest, TResponse>(TRequest transactionRequest);

    public Task<TResponse> PostDataRequestAsync<TRequest, TResponse>(TRequest dataRequest);
}
