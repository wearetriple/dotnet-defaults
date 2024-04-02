using System.Collections;
using NUnit.Framework;

namespace UnitTestingExample
{
    public class HobbiesTestCases : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData(new string[0] { }, ", zero hobbies")
                .SetName($"{nameof(UserServiceTests.GetDashboard_WhenHobbiesFound_ItShouldBeReturned)}(zero hobbies)");

            yield return new TestCaseData(new string[1] { "eat" }, ", eat")
                .SetName($"{nameof(UserServiceTests.GetDashboard_WhenHobbiesFound_ItShouldBeReturned)}(one hobby)");

            yield return new TestCaseData(new string[2] { "eat", "sleep" }, ", eat, sleep")
                .SetName($"{nameof(UserServiceTests.GetDashboard_WhenHobbiesFound_ItShouldBeReturned)}(two hobbies)");

            yield return new TestCaseData(new string[3] { "eat", "sleep", "repeat" }, ", eat, sleep, repeat")
                .SetName($"{nameof(UserServiceTests.GetDashboard_WhenHobbiesFound_ItShouldBeReturned)}(three hobbies)");
        }
    }
}
