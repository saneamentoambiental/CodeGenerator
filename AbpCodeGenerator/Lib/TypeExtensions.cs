using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AbpCodeGenerator.Lib
{
	public static class TypeExtensions
	{
		/// <summary>
		/// Determine whether a type is simple (String, Decimal, DateTime, etc) 
		/// or complex (i.e. custom class with public properties and methods).
		/// </summary>
		/// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
		public static bool IsSimpleType(
		this Type type)
		{
			return
				type.IsValueType ||
				type.IsPrimitive ||
				type.IsDecimal() ||
				type.IsInteger() ||
				type.IsDate() ||
				type.IsTime() ||
				new Type[] {
				typeof(String),
				
				typeof(Guid)
				}.Contains(type) ||
				Convert.GetTypeCode(type) != TypeCode.Object;
		}
		public static bool IsTime(
		this Type type)
		{
			return
				new Type[] {
				typeof(TimeSpan)
				}.Contains(type);
		}
		public static bool IsDate(
		this Type type)
		{
			return
				new Type[] {
				typeof(DateTime),
				typeof(DateTimeOffset)
				}.Contains(type);
		}
		
		public static bool IsDecimal(
		this Type type)
		{
			return 
				new Type[] {
				typeof(Decimal),
				typeof(double)
				}.Contains(type);
		}
		public static bool IsInteger(
		this Type type)
		{
			return
				new Type[] {
				typeof(int),
				typeof(long),
				}.Contains(type);
		}
	}
}
