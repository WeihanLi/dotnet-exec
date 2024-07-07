<Query Kind="Statements">
  <NuGetReference>WeihanLi.Npoi</NuGetReference>
  <Namespace>WeihanLi.Npoi</Namespace>
</Query>

CsvHelper.GetCsvText(new[]
{
	new 
	{
		Id = 1,
		Name = "test"
	}
}).Dump();