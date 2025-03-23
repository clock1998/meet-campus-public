
import { Slot } from 'expo-router';
import { SessionProvider } from '@/context/AuthContext';
import { QueryClient, QueryClientProvider} from '@tanstack/react-query'

export const queryClient = new QueryClient();

export default function RootLayout() {
  return (
    <QueryClientProvider client={queryClient}>
      <SessionProvider>
        <Slot />
      </SessionProvider>
    </QueryClientProvider>    
  );
}