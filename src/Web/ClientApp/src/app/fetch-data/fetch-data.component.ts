import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { WeatherForecastsClient, WeatherForecast } from '../web-api-client';

@Component({
  selector: 'app-fetch-data',
  standalone: true,
  imports: [CommonModule, CardModule, TableModule, TagModule, ProgressSpinnerModule],
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public forecasts: WeatherForecast[] = [];

  constructor(private client: WeatherForecastsClient) {
      client.getWeatherForecasts().subscribe({
          next: result => this.forecasts = result,
          error: error => console.error(error)
      });
  }
}
