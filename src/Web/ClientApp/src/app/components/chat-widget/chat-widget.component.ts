import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatService, Message } from '../../services/chat.service';
import { LanguageService } from '../../services/language.service';
import { AuthService } from '../../shared/auth/auth.service';
import { ChatHeaderComponent } from './chat-header/chat-header.component';
import { ChatMessagesComponent } from './chat-messages/chat-messages.component';
import { ChatInputComponent } from './chat-input/chat-input.component';
import { QuickRepliesComponent } from './quick-replies/quick-replies.component';

@Component({
  selector: 'app-chat-widget',
  standalone: true,
  imports: [
    CommonModule,
    ChatHeaderComponent,
    ChatMessagesComponent,
    ChatInputComponent,
    QuickRepliesComponent
  ],
  templateUrl: './chat-widget.component.html',
  styleUrls: ['./chat-widget.component.css']
})
export class ChatWidgetComponent implements OnInit {
  messages$  = this.chatService.messages$;
  isLoading$ = this.chatService.isLoading$;
  isOpen     = false;  // Fermé par défaut — bouton FAB en bas

  constructor(
    private chatService: ChatService,
    private languageService: LanguageService,
    private authService: AuthService
  ) {}

  isLoggedIn(): boolean {
    return this.authService.currentUser() !== null;
  }

  ngOnInit(): void {}

  onSendMessage(message: string): void {
    if (message.trim()) this.chatService.addUserMessage(message);
  }

  onQuickReplyClick(reply: string): void {
    if (reply.trim()) this.chatService.addUserMessage(reply);
  }

  toggleChat(): void {
    this.isOpen = !this.isOpen;
  }

  onClearChat(): void {
    this.chatService.clearMessages();
  }

  isRTL(): boolean {
    return this.languageService.isRTL();
  }
}
