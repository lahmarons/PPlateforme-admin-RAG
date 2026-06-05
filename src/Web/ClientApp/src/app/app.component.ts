import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LayoutService } from './layout/service/layout.service';
import { ChatWidgetComponent } from './components/chat-widget/chat-widget.component';
import { ChatService } from './services/chat.service';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ChatWidgetComponent],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  title = 'IdaraTech';
  layoutService = inject(LayoutService);
  private chatService = inject(ChatService);

  ngOnInit(): void {
    // Configure the ChatService using environment settings
    this.chatService.configure({
      apiUrl: environment.apiUrl,
      apiKey: environment.apiKey,
      language: 'fr'
    });
  }
}
