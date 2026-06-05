import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { ChatService, Message, QuickReply, Language } from '../../services/chat.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-agent-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonModule, TagModule, TextareaModule],
  templateUrl: './agent-chatbot.component.html'
})
export class AgentChatbotComponent implements OnInit, OnDestroy {
  private chatService = inject(ChatService);

  question = '';
  messages: Message[] = [];
  isLoading = false;
  currentLang: Language = 'fr';
  quickReplies: QuickReply[] = [];

  private subscriptions = new Subscription();

  ngOnInit(): void {
    this.subscriptions.add(
      this.chatService.messages$.subscribe(msgs => {
        this.messages = msgs;
      })
    );

    this.subscriptions.add(
      this.chatService.isLoading$.subscribe(loading => {
        this.isLoading = loading;
      })
    );

    this.subscriptions.add(
      this.chatService.language$.subscribe(lang => {
        this.currentLang = lang;
        this.quickReplies = this.chatService.getQuickReplies(lang);
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  send(): void {
    const text = this.question.trim();
    if (!text || this.isLoading) {
      return;
    }

    this.chatService.addUserMessage(text);
    this.question = '';
  }

  selectQuickReply(reply: QuickReply): void {
    this.chatService.addUserMessage(reply.text);
  }

  changeLanguage(lang: Language): void {
    this.chatService.changeLanguage(lang);
  }

  clearChat(): void {
    this.chatService.clearMessages();
  }
}
