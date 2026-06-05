import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService, Message, QuickReply, Language } from '../../services/chat.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-chat-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-widget.component.html',
  styleUrls: ['./chat-widget.component.scss']
})
export class ChatWidgetComponent implements OnInit, OnDestroy, AfterViewChecked {
  private chatService = inject(ChatService);

  @ViewChild('messageScrollContainer') private scrollContainer!: ElementRef;

  isOpen = false;
  messageText = '';
  messages: Message[] = [];
  isLoading = false;
  currentLang: Language = 'fr';
  quickReplies: QuickReply[] = [];

  private subscriptions = new Subscription();

  ngOnInit(): void {
    // Subscribe to message stream
    this.subscriptions.add(
      this.chatService.messages$.subscribe(msgs => {
        this.messages = msgs;
        this.scrollToBottom();
      })
    );

    // Subscribe to loading state
    this.subscriptions.add(
      this.chatService.isLoading$.subscribe(loading => {
        this.isLoading = loading;
        this.scrollToBottom();
      })
    );

    // Subscribe to language changes
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

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  toggleChat(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.scrollToBottom();
    }
  }

  sendMessage(): void {
    const text = this.messageText.trim();
    if (!text) return;

    this.chatService.addUserMessage(text);
    this.messageText = '';
  }

  selectQuickReply(reply: QuickReply): void {
    this.chatService.addUserMessage(reply.text);
  }

  changeLanguage(lang: Language): void {
    this.chatService.changeLanguage(lang);
  }

  clearChat(): void {
    if (confirm('Voulez-vous vraiment effacer la conversation ? / هل تريد حقاً مسح المحادثة؟')) {
      this.chatService.clearMessages();
    }
  }

  private scrollToBottom(): void {
    try {
      if (this.scrollContainer) {
        this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
      }
    } catch (err) {
      // Ignore scroll errors
    }
  }
}
