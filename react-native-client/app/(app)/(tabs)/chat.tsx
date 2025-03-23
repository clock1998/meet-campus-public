import { useSession } from '@/context/AuthContext';
import { SignalRService } from '@/services/signalRService';
import React, { useEffect, useState } from 'react';
import { View, Text, Button, StyleSheet, FlatList } from 'react-native';

export default function ChatScreen() {
  const { userSession } = useSession();
  const [user, setUser] = useState<{ userId: string; username: string }>();
  const [messages, setMessages] = useState<{ userId: string; username: string }[]>([]);
  const signalRService = new SignalRService(userSession!.token);

  useEffect(() => {
    let connection = signalRService.connect();
    signalRService.onUserConnected((userId: string, username: string) => {
      setUser({ userId, username });
      // setMessages(prev => [...prev, { userId, username }]);
    });
    // Clean up the event listener on unmount
    return () => {
      connection?.stop();
    };
  }, []);

  return (
    <View style={styles.container}>
      <Text style={styles.title}>Chat Screen</Text>
      <Text style={styles.title}>{user?.userId} {user?.username}</Text>
      <FlatList
        data={messages}
        keyExtractor={(_, index) => index.toString()}
        renderItem={({ item }) => (
          <Text>{`${item.userId}: ${item.username}`}</Text>
        )}
      />
      <Button
        title="Send Test Message"
        onPress={() => {
          // Example of sending a message; adjust according to your ChatHub method
          signalRService?.sendMessageToRoom({userId: user!.userId, roomId: user!.userId, content: "Tes"})
        }}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, padding: 20 },
  title: { fontSize: 24, marginBottom: 20 }
});