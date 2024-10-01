
using Xunit;
//Optional
//[assembly: CollectionBehavior(DisableTestParallelization = true)]
[assembly: TestCaseOrderer("Xunit.Extensions.Ordering.TestCaseOrderer", "Xunit.Extensions.Ordering")]
[assembly: TestFramework("Xunit.Extensions.Ordering.TestFramework", "Xunit.Extensions.Ordering")] 