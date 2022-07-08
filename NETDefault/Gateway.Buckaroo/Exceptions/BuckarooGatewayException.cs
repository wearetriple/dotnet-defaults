namespace Gateway.Buckaroo.Exceptions;

internal class BuckarooGatewayException : GatewayException
{
    public BuckarooGatewayException()
    {
    }

    public BuckarooGatewayException(string message) : base(message)
    {
    }

    public BuckarooGatewayException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
