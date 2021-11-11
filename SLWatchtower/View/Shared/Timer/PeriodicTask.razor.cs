using Microsoft.AspNetCore.Components;

namespace SLWatchtower.View.Shared.Timer
{
    public partial class PeriodicTask : IDisposable
    {
		[Parameter]
		public int Period { get; set; } = 60;

		[Parameter]
		public bool OneShot { get; set; }

		[Parameter]
		public Action Action { get; set; }

		private System.Timers.Timer _timer = new System.Timers.Timer();

		protected override void OnAfterRender(bool firstRender)
		{
			if (firstRender)
			{
				_timer.Interval = Period;
				_timer.AutoReset = !OneShot;
				_timer.Elapsed += _timer_Elapsed;
				_timer.Start();
			}

			base.OnAfterRender(firstRender);
		}

		private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Action?.Invoke();
		}

		public void Dispose()
		{
			_timer.Stop();
			_timer.Dispose();
		}
	}
}
