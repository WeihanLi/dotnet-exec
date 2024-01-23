// Copyright (c) 2022-2024 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

namespace Exec.Contracts;

public static class ExitCodes
{
    public const int OperationCancelled = -1;
    public const int Success = 0;
    
    public const int InvalidScript = -1;
    public const int FetchError = -2;
    public const int CompileError = -3;
    public const int ExecuteError = -4;
    public const int ExecuteException = -5;
}
