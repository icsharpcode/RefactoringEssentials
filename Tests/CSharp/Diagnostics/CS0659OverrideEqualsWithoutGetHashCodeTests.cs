namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    //	[TestFixture]
    //	public class CS0659OverrideEqualsWithoutGetHashCodeTests : InspectionActionTestBase
    //	{
    //		[Test]
    //		public void WithoutGetHashCode()
    //		{
    //			var input = @"
    //namespace application
    //{
    //	public class BaseClass
    //	{
    //		public override bool $Equals$(object o)
    //		{
    //			return base.Equals(o);
    //		}
    //	}
    //}";
    //			var output = @"
    //namespace application
    //{
    //	public class BaseClass
    //	{
    //		public override bool Equals(object o)
    //		{
    //			return base.Equals(o);
    //		}

    //        public override int GetHashCode()
    //        {
    //            return base.GetHashCode();
    //        }
    //    }
    //}";
    //			Analyze<CS0659ClassOverrideEqualsWithoutGetHashCode>(input, output);
    //		}

    //		[Test]
    //		public void WithoutEquals()
    //		{
    //			var input = @"

    //namespace application
    //{
    //	public class Program
    //	{
    //		public bool Equals(Program o)
    //		{
    //			return false;
    //		}
    //	}
    //}";
    //			Analyze<CS0659ClassOverrideEqualsWithoutGetHashCode>(input);
    //		}

    //		[Test]
    //		public void PartialClass()
    //		{
    //			var input = @"
    //namespace application
    //{
    //	public partial class BaseClass
    //	{
    //		public override bool Equals(object o)
    //		{
    //			return base.Equals(o);
    //		}
    //	}
    //	public partial class BaseClass
    //	{
    //		public override int GetHashCode()
    //		{
    //			return base.GetHashCode();
    //		}
    //	}
    //}";

    //			Analyze<CS0659ClassOverrideEqualsWithoutGetHashCode>(input);
    //		}

    //		[Test]
    //		public void WithGetHashCode()
    //		{
    //			var input = @"
    //namespace application
    //{
    //	public class Program
    //	{
    //		public override int GetHashCode()
    //		{
    //			return 1;
    //		}
    //		public override bool Equals(Object o)
    //		{
    //			return false;
    //		}
    //	}
    //}";
    //			Analyze<CS0659ClassOverrideEqualsWithoutGetHashCode>(input);
    //		}

    //		[Test]
    //		public void ResharperDisable()
    //		{
    //			var input = @"
    //namespace application
    //{
    //	public class Program
    //	{
    ////Resharper disable CSharpWarnings::CS0659
    //		public override bool Equals(Object o)
    //		{
    //			return false;
    //		}
    ////Resharper restore CSharpWarnings::CS0659
    //	}
    //}";
    //			Analyze<CS0659ClassOverrideEqualsWithoutGetHashCode>(input);
    //		}

    //		[Test]
    //		public void TestPragmaSuppression()
    //		{
    //			var input = @"
    //namespace application
    //{
    //	public class Program
    //	{
    //#pragma warning disable 0659
    //		public override bool Equals(Object o)
    //		{
    //			return false;
    //		}
    //#pragma warning restore 0659
    //	}
    //}";
    //			Analyze<CS0659ClassOverrideEqualsWithoutGetHashCode>(input);
    //		}
    //	}
}