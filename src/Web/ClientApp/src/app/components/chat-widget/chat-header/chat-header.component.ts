import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService, Language } from '../../../services/language.service';

@Component({
  selector: 'app-chat-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './chat-header.component.html',
  styleUrls: ['./chat-header.component.css']
})
export class ChatHeaderComponent {
  @Output() onClearChat  = new EventEmitter<void>();
  @Output() onCloseChat  = new EventEmitter<void>();

  currentLanguage$ = this.languageService.currentLanguage$;

  constructor(private languageService: LanguageService) {}

  onLanguageChange(language: Language): void {
    this.languageService.setLanguage(language);
  }

  onClearClick(): void {
    this.onClearChat.emit();
  }

  onCloseWidget(): void {
    this.onCloseChat.emit();
  }
}
