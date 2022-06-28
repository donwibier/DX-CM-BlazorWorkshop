using Blazor.FluxorTest.Client.Features.Counter.Store;
using Blazor.FluxorTest.Shared;
using Fluxor;
using System.Net.Http.Json;

namespace Blazor.FluxorTest.Client.Features.Weather.Store
{
    public record WeatherState
    {
        public bool Initialized { get; init; }
        public bool Loading { get; init; }
        public WeatherForecast[] Forecasts { get; init; }
    }

    public class WeatherFeature : Feature<WeatherState>
    {
        public override string GetName() => "Weather";

        protected override WeatherState GetInitialState()
        {
            return new WeatherState
            {
                Initialized = false,
                Loading = false,
                Forecasts = new WeatherForecast[] { }
            };
        }
    }

    public class WeatherLoadForecastsAction { }

    public class WeatherSetForecastsAction
    {
        public WeatherSetForecastsAction(WeatherForecast[] forecasts)
        {
            Forecasts = forecasts;
        }

        public WeatherForecast[] Forecasts { get; }
    }

    public static class WeatherReducers
    {
        [ReducerMethod]
        public static WeatherState OnSetForecasts(WeatherState state, WeatherSetForecastsAction action)
        {
            return state with
            {
                Forecasts = action.Forecasts,
                Loading = false,
                Initialized = true
            };
        }

        [ReducerMethod(typeof(WeatherLoadForecastsAction))]
        public static WeatherState OnLoadForecasts(WeatherState state)
        {
            return state with
            {
                Loading = true
            };
        }

    }

    public class WeatherEffects
    {
        private readonly HttpClient http;
        private readonly IState<CounterState> counterState;
        public WeatherEffects(HttpClient http, IState<CounterState> counterState)
        {
            this.http = http;
            this.counterState = counterState;
        }

        [EffectMethod(typeof(WeatherLoadForecastsAction))]
        public async Task LoadForecasts(IDispatcher dispatcher)
        {
            //dispatcher.Dispatch(new WeatherSetLoadingAction(true));
            var forecasts = await http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
            await Task.Delay(1000);
            dispatcher.Dispatch(new WeatherSetForecastsAction(forecasts ?? new WeatherForecast[] { }));
            
        }

        [EffectMethod(typeof(CounterIncrementAction))]
        public async Task LoadForecastsOnIncrement(IDispatcher dispatcher)
        {            
            if (counterState.Value.CurrentCount % 10 == 0)
            {
                dispatcher.Dispatch(new WeatherLoadForecastsAction());
            }
            await Task.Yield();
        }
    }

}
