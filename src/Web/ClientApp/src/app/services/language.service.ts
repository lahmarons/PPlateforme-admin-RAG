import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ChatService } from './chat.service';

export type Language = 'fr' | 'ar' | 'tn';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private languageSubject = new BehaviorSubject<Language>('fr');
  currentLanguage$ = this.languageSubject.asObservable();

  private translations: { [key: string]: { fr: string; ar: string; tn: string } } = {
    placeholder: {
      fr: 'Posez votre question administrative...',
      ar: 'اطرح سؤالك الإداري...',
      tn: 'اكتب سؤالك هنا...'
    },
    send: {
      fr: 'Envoyer',
      ar: 'إرسال',
      tn: 'ابعث'
    },
    quickRepliesLabel: {
      fr: 'Questions fréquentes',
      ar: 'أسئلة شائعة',
      tn: 'أسئلة متكررة'
    }
  };

  constructor(private chatService: ChatService) {}

  setLanguage(language: Language): void {
    this.languageSubject.next(language);
    this.chatService.changeLanguage(language);
  }

  getCurrentLanguage(): Language {
    return this.languageSubject.value;
  }

  translate(key: string): string {
    const lang = this.languageSubject.value;
    return this.translations[key]?.[lang] || key;
  }

  isRTL(): boolean {
    const lang = this.languageSubject.value;
    return lang === 'ar' || lang === 'tn';
  }
}