export interface Transaction {
    offerUsername?: string;
    offer: string;
    offerIceCandidates: string[];
    answerUsername?: string;
    answer?: string;
    answerIceCandidates: IceCandidate[];
  }
  export interface IceCandidate{
    email: string;
    candiate: string;
    isMyOffer: boolean;
  }