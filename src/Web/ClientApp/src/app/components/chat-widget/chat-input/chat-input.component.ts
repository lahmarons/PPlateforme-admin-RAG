import { Component, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LanguageService } from '../../../services/language.service';

@Component({
  selector: 'app-chat-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-input.component.html',
  styleUrls: ['./chat-input.component.css']
})
export class ChatInputComponent {
  @Output() onSendMessage = new EventEmitter<string>();
  @ViewChild('inputField') inputField!: ElementRef;

  message = '';

  constructor(private languageService: LanguageService) {}

  onSend(): void {
    const text = this.message.trim();
    if (text) {
      this.onSendMessage.emit(text);
      this.message = '';
      // ✅ Remettre le focus sur l'input après envoi et s'assurer qu'il est visible
      setTimeout(() => {
        const el = this.inputField?.nativeElement;
        if (el) {
          try {
            el.focus();
            el.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
          } catch {
            el.focus();
          }
        }
      }, 150);
    }
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.onSend();
    }
  }

  getPlaceholder(): string {
    return this.languageService.translate('placeholder');
  }

  getSendButtonText(): string {
    return this.languageService.translate('send');
  }

  onWheel(event: WheelEvent): void {
    // Forward vertical wheel scroll to messages container so mouse wheel works
    const messages = document.querySelector('.chat-widget .messages-container') as HTMLElement | null;
    if (messages) {
      messages.scrollTop += event.deltaY;
      event.preventDefault();
    }
  }
}