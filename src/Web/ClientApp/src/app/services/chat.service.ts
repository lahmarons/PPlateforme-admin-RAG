// src/app/services/chat.service.ts  —  VERSION INTÉGRATION
// Service Angular adapté pour s'intégrer dans n'importe quelle application
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';

export type Language = 'fr' | 'ar' | 'tn';

export interface Message {
  id: string;
  text: string;
  sender: 'user' | 'bot';
  timestamp: Date;
  isTyping?: boolean;
}

export interface QuickReply {
  id: string;
  text: string;
}

export interface ChatConfig {
  apiUrl?: string;    // override depuis l'application parente
  apiKey?: string;    // clé API fournie par l'application parente
  language?: Language;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private apiUrl: string = environment.apiUrl;
  private apiKey: string = environment.apiKey || '';
  private sessionId = this.generateId();

  private messagesSubject    = new BehaviorSubject<Message[]>([]);
  public  messages$          = this.messagesSubject.asObservable();

  private languageSubject    = new BehaviorSubject<Language>('fr');
  public  language$          = this.languageSubject.asObservable();

  private isLoadingSubject   = new BehaviorSubject<boolean>(false);
  public  isLoading$         = this.isLoadingSubject.asObservable();

  private counter = 0;

  constructor(private http: HttpClient) {
    this.initializeChat('fr');
  }

  /**
   * Configure le service depuis l'application parente.
   * À appeler AVANT toute interaction, typiquement dans AppComponent.ngOnInit()
   *
   * Exemple :
   *   this.chatService.configure({
   *     apiUrl: 'https://api.mondomaine.com',
   *     apiKey: 'ma_cle_api_secrete',
   *     language: 'fr'
   *   });
   */
  configure(config: ChatConfig): void {
    if (config.apiUrl) this.apiUrl = config.apiUrl;
    if (config.apiKey) this.apiKey = config.apiKey;
    if (config.language) this.initializeChat(config.language);
  }

  // ── Envoi de message ───────────────────────────────────────

  addUserMessage(text: string): void {
    if (this.isLoadingSubject.value) return;
    this.messagesSubject.next([
      ...this.messagesSubject.value,
      { id: this.generateId(), text, sender: 'user', timestamp: new Date() }
    ]);
    this.sendToBackend(text);
  }

  private sendToBackend(userMessage: string): void {
    this.isLoadingSubject.next(true);

    // Typing indicator
    const typingId = this.generateId();
    this.messagesSubject.next([
      ...this.messagesSubject.value,
      { id: typingId, text: '', sender: 'bot', timestamp: new Date(), isTyping: true }
    ]);

    const lang = this.languageSubject.value === 'fr' ? 'fr' : 'ar';

    const history = this.messagesSubject.value
      .filter(m => !m.isTyping)
      .slice(-8)
      .map(m => ({ role: m.sender === 'user' ? 'user' : 'assistant', content: m.text }));

    // ✅ Header X-API-Key injecté automatiquement
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      ...(this.apiKey ? { 'X-API-Key': this.apiKey } : {})
    });

    this.http.post<any>(
      `${this.apiUrl}/chat`,
      { message: userMessage, session_id: this.sessionId, language: lang, use_rag: true, history },
      { headers }
    ).subscribe({
      next: (res) => {
        const msgs = this.messagesSubject.value.filter(m => !m.isTyping);
        const answer = this.cleanResponse(res.answer || res.response || '');
        this.messagesSubject.next([
          ...msgs,
          { id: this.generateId(), text: answer, sender: 'bot', timestamp: new Date() }
        ]);
        this.isLoadingSubject.next(false);
      },
      error: (err) => {
        console.error('Chat error:', err);
        const msgs = this.messagesSubject.value.filter(m => !m.isTyping);
        const isUnauthorized = err.status === 401;
        const errText = isUnauthorized
          ? '🔐 Erreur d\'authentification. Vérifiez votre clé API.'
          : (lang === 'ar'
              ? '⚠️ عذراً، حدث خطأ. تأكد أن الـ backend يعمل على المنفذ 8001.'
              : '⚠️ Désolé, une erreur s\'est produite. Vérifiez que le backend tourne.');
        this.messagesSubject.next([
          ...msgs,
          { id: this.generateId(), text: errText, sender: 'bot', timestamp: new Date() }
        ]);
        this.isLoadingSubject.next(false);
      }
    });
  }

  // ── Utilitaires ────────────────────────────────────────────

  private initializeChat(language: Language): void {
    const greetings: { [key in Language]: string } = {
      fr: 'Bonjour ! Je suis IDARATECH, votre conseiller administratif tunisien.\nComment puis-je vous aider aujourd\'hui ? 😊',
      ar: 'مرحباً ! أنا IDARATECH، مستشارك الإداري التونسي.\nكيف يمكنني مساعدتك اليوم؟ 😊',
      tn: 'مرحبا ! أنا IDARATECH، المستشار الإداري التونسي.\nكيفاش نجم نساعدك اليوم؟ 😊'
    };
    this.messagesSubject.next([{
      id: this.generateId(),
      text: greetings[language],
      sender: 'bot',
      timestamp: new Date()
    }]);
    this.languageSubject.next(language);
    this.isLoadingSubject.next(false);
  }

  private cleanResponse(text: string): string {
    if (!text) return 'Je n\'ai pas pu traiter votre demande.';
    let cleaned = text
      .replace(/### RÉPONSE PROFESSIONNELLE/gi, '')
      .replace(/### RÉPONSE/gi, '')
      .replace(/Réponse professionnelle\s*:/gi, '')
      .replace(/RÉPONSE\s*:/gi, '')
      .replace(/\[INST\]/gi, '')
      .replace(/\[\/INST\]/gi, '')
      .trim();
    if (cleaned.toLowerCase().startsWith('utilisateur:') ||
        cleaned.toLowerCase().startsWith('user:')) {
      const lines = cleaned.split('\n');
      cleaned = lines
        .filter(l => !l.toLowerCase().startsWith('utilisateur:') &&
                     !l.toLowerCase().startsWith('user:') &&
                     !l.toLowerCase().startsWith('assistant:') &&
                     l.trim().length > 0)
        .join('\n').trim();
    }
    return cleaned || 'Je n\'ai pas pu traiter votre demande.';
  }

  changeLanguage(language: Language): void {
    this.languageSubject.next(language);
    this.initializeChat(language);
  }

  getQuickReplies(language: Language): QuickReply[] {
    const replies: { [key in Language]: string[] } = {
      fr: ['Passeport 🛂', 'CIN 🪪', 'Permis 🚗', 'CNSS 🏥', 'Université 🎓'],
      ar: ['جواز السفر 🛂', 'بطاقة هوية 🪪', 'رخصة القيادة 🚗', 'الكنص 🏥', 'الجامعة 🎓'],
      tn: ['باسبور 🛂', 'بطاقة 🪪', 'رخصة 🚗', 'كنص 🏥', 'جامعة 🎓']
    };
    return replies[language].map((text, i) => ({ id: `qr-${i}`, text }));
  }

  clearMessages(): void {
    this.sessionId = this.generateId();
    this.isLoadingSubject.next(false);
    this.initializeChat(this.languageSubject.value);
  }

  private generateId(): string {
    return `msg-${++this.counter}-${Date.now()}`;
  }
}
