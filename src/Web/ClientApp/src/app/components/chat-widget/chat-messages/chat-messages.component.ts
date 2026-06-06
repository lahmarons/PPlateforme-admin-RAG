import {
  Component, Input, OnChanges, ViewChild,
  ElementRef, AfterViewChecked, AfterViewInit, SimpleChanges
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Message } from '../../../services/chat.service';

@Component({
  selector: 'app-chat-messages',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './chat-messages.component.html',
  styleUrls: ['./chat-messages.component.css']
})
export class ChatMessagesComponent implements OnChanges, AfterViewChecked, AfterViewInit {
  @Input() messages: Message[] | null = [];
  @Input() isLoading: boolean | null = false;
  @ViewChild('messagesContainer') container!: ElementRef;
  private shouldScroll = false;
  isAtBottom = true;
  showNewMessages = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['messages']) {
      // If user is at bottom, auto-scroll; otherwise show "new messages" indicator
      if (this.isAtBottom) {
        this.shouldScroll = true;
        this.showNewMessages = false;
      } else {
        this.showNewMessages = true;
      }
    }
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll && this.isAtBottom) {
      this.shouldScroll = false;
      setTimeout(() => this.scrollToBottom(), 80);
    }
  }

  // called from template button to force scroll
  scrollToBottom(): void {
    const el = this.container?.nativeElement;
    if (el) {
      try {
        el.scrollTo({ top: el.scrollHeight, behavior: 'smooth' });
      } catch {
        el.scrollTop = el.scrollHeight;
      }
      this.showNewMessages = false;
      this.isAtBottom = true;
    }
  }

  // track manual scroll position is done via native listener attached to the container

  // attach native listener after view init via container element
  private attachScrollListener(): void {
    const el = this.container?.nativeElement as HTMLElement | null;
    if (!el) return;
    el.addEventListener('scroll', () => {
      const atBottom = el.scrollHeight - (el.scrollTop + el.clientHeight) < 20;
      this.isAtBottom = atBottom;
      if (atBottom) this.showNewMessages = false;
    });
  }

  // ensure listener installed when ViewChild ready
  ngAfterViewInit(): void {
    this.attachScrollListener();
  }
  
}
