﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    public interface ISymbolScope
    {
        Symbol ResolveSymbol(string name);
    }
}
