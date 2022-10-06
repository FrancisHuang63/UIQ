namespace UIQ.ViewModels
{
	public class TyphoonSetDataViewModel
	{
		/// <summary>
		/// 颱風/TD 名稱
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 緯度
		/// </summary>
		public double? Lat { get; set; }

		/// <summary>
		/// 經度
		/// </summary>
		public double? Lng { get; set; }

		/// <summary>
		/// 6hr前 緯度
		/// </summary>
		public double? LatBefore6Hours { get; set; }

		/// <summary>
		/// 6hr前 經度
		/// </summary>
		public double? LngBefore6Hours { get; set; }

		/// <summary>
		/// 中心氣壓
		/// </summary>
		public int? CenterPressure { get; set; }

		/// <summary>
		/// 七級風暴風半徑
		/// </summary>
		public int? Radius15MPerS { get; set; }

		/// <summary>
		/// 最大風速
		/// </summary>
		public int? MaximumSpeed { get; set; }

		/// <summary>
		/// 十級風暴風半徑
		/// </summary>
		public int? Radius25MPerS { get; set; }
	}
}