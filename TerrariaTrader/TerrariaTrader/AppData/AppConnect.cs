using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaTrader.AppData;

namespace TerrariaTrader.AppData
{
    internal class AppConnect
    {
        private static Entities1 _model01;

        public static Entities1 model01
        {
            get
            {
                if (_model01 == null)
                {
                    _model01 = new Entities1();
                }
                return _model01;
            }
        }

        // Метод для очистки контекста при завершении приложения
        public static void Dispose()
        {
            _model01?.Dispose();
            _model01 = null;
        }
    }
}
