import { Component } from '@angular/core';
import { AppLayout } from './layout/component/app.layout'; 

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [AppLayout], 
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'IdaraTech';
}