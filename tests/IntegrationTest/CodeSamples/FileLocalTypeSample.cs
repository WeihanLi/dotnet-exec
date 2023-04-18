// Copyright (c) 2022-2023 Weihan Li. All rights reserved.
// Licensed under the Apache license version 2.0 http://www.apache.org/licenses/LICENSE-2.0

using System.Text.Json;
using System.Text.Json.Serialization;

var personJsonWithoutId = JsonSerializer.Serialize(new { Id = 1, Name = "1234", Age = 10 });
try
{
    var p = JsonSerializer.Deserialize<Person>(personJsonWithoutId);
    Console.WriteLine(p.ToString());
}
catch (Exception e)
{
    Console.WriteLine(e);
}

try
{
    var p = JsonSerializer.Deserialize<Person>(personJsonWithoutId,
        new JsonSerializerOptions() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow });
    Console.WriteLine(p.ToString());
}
catch (Exception e)
{
    Console.WriteLine(e);
}

file record Person(int Id, string Name);
