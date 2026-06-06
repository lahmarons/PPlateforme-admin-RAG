import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatService, QuickReply, Language } from '../../../services/chat.service';
import { LanguageService } from '../../../services/language.service';

@Component({
  selector: 'app-quick-replies',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './quick-replies.component.html',
  styleUrls: ['./quick-replies.component.css']
})
export class QuickRepliesComponent implements OnInit {
  @Output() onQuickReplyClick = new EventEmitter<string>();

  quickReplies: QuickReply[] = [];

  constructor(
    private chatService: ChatService,
    private languageService: LanguageService
  ) {}

  ngOnInit(): void {
    // Charger les quick replies au démarrage
    this.loadReplies(this.languageService.getCurrentLanguage());

    // Réagir aux changements de langue
    this.languageService.currentLanguage$.subscribe(lang => {
      this.loadReplies(lang);
    });
  }

  private loadReplies(lang: Language): void {
    this.quickReplies = this.chatService.getQuickReplies(lang);
  }

  onReplyClick(reply: QuickReply): void {
    this.onQuickReplyClick.emit(reply.text);
  }

  getLabel(): string {
    const lang = this.languageService.getCurrentLanguage();
    const labels: { [key in Language]: string } = {
      fr: 'Questions fréquentes',
      ar: 'أسئلة شائعة',
      tn: 'أسئلة متكررة'
    };
    return labels[lang];
  }
}
