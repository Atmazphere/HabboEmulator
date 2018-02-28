using System;
using MySql.Data.MySqlClient;

namespace Emulator.Database.Interfaces
{
    public interface IDatabaseClient : IDisposable
    {
        void connect();
        void disconnect();
        IQueryAdapter GetQueryReactor();
        MySqlCommand createNewCommand();
        void reportDone();
    }
}