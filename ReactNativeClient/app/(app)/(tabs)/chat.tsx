import { useSession } from '@/context/AuthContext';
import React, { useEffect, useState, useRef } from 'react';
import { View, Text, StyleSheet, TouchableOpacity, ActivityIndicator, Modal, Dimensions } from 'react-native';
import { useRouter } from 'expo-router';
import { FlashList } from '@shopify/flash-list';
import { useSignalR } from '@/context/SignalRContext';
import { ChatRoom } from '@/services/signalRService';

const { height: screenHeight } = Dimensions.get('window');

export default function ChatRoomsScreen() {
  const { userSession } = useSession();
  const [isLoading, setIsLoading] = useState(false);
  const [dropdownVisible, setDropdownVisible] = useState(false);
  const [dropdownPosition, setDropdownPosition] = useState({ x: 0, y: 0 });
  const [selectedRoom, setSelectedRoom] = useState<ChatRoom | null>(null);
  const { signalRService, isConnected, onlineUsers, chatRooms } = useSignalR();
  const router = useRouter();

  useEffect(() => {
    if (!signalRService || !isConnected) return;
    console.log(chatRooms)

  }, [signalRService, isConnected, onlineUsers, chatRooms, router]);

  const handleRoomPress = (roomId: string) => {
    setDropdownVisible(false);
    router.push({
      pathname: '/(app)/chat/[id]',
      params: { id: roomId },
    });
  };

  const handleRoomLongPress = (room: ChatRoom, event: any) => {
    const { pageY } = event.nativeEvent;
    const dropdownHeight = 200; // Approximate height of dropdown
    const dropdownWidth = 120; // Approximate width of dropdown
    
    // Calculate position - show above if near bottom, below if near top
    const showAbove = pageY > screenHeight / 2;
    const y = showAbove ? pageY - dropdownHeight - 10 : pageY + 50;
    
    // Position on the right side of the screen
    const x = Dimensions.get('window').width - dropdownWidth - 20;
    
    setDropdownPosition({ x, y });
    setSelectedRoom(room);
    setDropdownVisible(true);
  };

  const handleDropdownOption = (action: string) => {
    if (!selectedRoom) return;
    
    setDropdownVisible(false);
    
    switch (action) {
      case 'rename':
        handleRenameRoom(selectedRoom);
        break;
      case 'pin':
        handlePinRoom(selectedRoom);
        break;
      case 'delete':
        handleDeleteRoom(selectedRoom.id);
        break;
      case 'report':
        handleReportRoom(selectedRoom);
        break;
    }
  };

  const handleRenameRoom = (room: ChatRoom) => {
    // TODO: Implement rename functionality
    console.log('Renaming room:', room.id);
  };

  const handlePinRoom = (room: ChatRoom) => {
    // TODO: Implement pin functionality
    console.log('Pinning room:', room.id);
  };

  const handleDeleteRoom = async (roomId: string) => {
    try {
      // TODO: Implement delete room functionality
      await signalRService?.deleteRoom(roomId);
      console.log('Deleting room:', roomId);
    } catch (error) {
      console.error('Failed to delete room:', error);
    }
  };

  const handleReportRoom = (room: ChatRoom) => {
    // TODO: Implement report functionality
    console.log('Reporting room:', room.id);
  };

  if (isLoading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color="#0000ff" />
        <Text style={styles.loadingText}>Loading chat rooms...</Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <FlashList
        estimatedItemSize={80}
        contentContainerStyle={styles.roomList}
        data={chatRooms}
        keyExtractor={(item: ChatRoom) => item.id}
        renderItem={({ item }: { item: ChatRoom }) => (
          <TouchableOpacity 
            style={styles.roomContainer}
            onPress={() => handleRoomPress(item.id)}
            onLongPress={(event) => handleRoomLongPress(item, event)}
          >
            <View style={styles.roomInfo}>
              <Text style={styles.roomName}>{item.name}</Text>
              {item.lastMessage && (
                <Text style={styles.lastMessage} numberOfLines={1}>
                  {item.lastMessage.username}: {item.lastMessage.content}
                </Text>
              )}
            </View>
            {item.lastMessage && (
              <Text style={styles.timestamp}>
                {new Date(item.lastMessage.updated).toLocaleTimeString()}
              </Text>
            )}
          </TouchableOpacity>
        )}
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Text style={styles.emptyText}>No chat rooms yet</Text>
          </View>
        }
      />

      {/* Custom Dropdown */}
      <Modal
        visible={dropdownVisible}
        transparent={true}
        animationType="fade"
        onRequestClose={() => setDropdownVisible(false)}
      >
        <TouchableOpacity
          style={styles.modalOverlay}
          activeOpacity={1}
          onPress={() => setDropdownVisible(false)}
        >
          <View style={[styles.dropdown, { top: dropdownPosition.y, left: dropdownPosition.x }]}>
          <TouchableOpacity
              style={styles.dropdownOption}
              onPress={() => handleDropdownOption('pin')}
            >
              <Text style={styles.dropdownOptionText}>Pin</Text>
            </TouchableOpacity>

            <TouchableOpacity
              style={styles.dropdownOption}
              onPress={() => handleDropdownOption('rename')}
            >
              <Text style={styles.dropdownOptionText}>Rename</Text>
            </TouchableOpacity>
            

            <TouchableOpacity
              style={[styles.dropdownOption, styles.deleteOption]}
              onPress={() => handleDropdownOption('delete')}
            >
              <Text style={[styles.dropdownOptionText, styles.deleteOptionText]}>Delete</Text>
            </TouchableOpacity>
            
            <TouchableOpacity
              style={styles.dropdownOption}
              onPress={() => handleDropdownOption('report')}
            >
              <Text style={styles.dropdownOptionText}>Report</Text>
            </TouchableOpacity>
          </View>
        </TouchableOpacity>
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#fff',
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#fff',
  },
  loadingText: {
    marginTop: 10,
    fontSize: 16,
    color: '#666',
  },
  header: {
    padding: 20,
    borderBottomWidth: 1,
    borderBottomColor: '#eee',
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
  },
  roomList: {
    padding: 10,
  },
  roomContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 15,
    backgroundColor: '#f8f8f8',
    borderRadius: 10,
    marginBottom: 8,
  },
  roomInfo: {
    flex: 1,
  },
  roomName: {
    fontSize: 16,
    fontWeight: '500',
    marginBottom: 4,
  },
  lastMessage: {
    fontSize: 14,
    color: '#666',
  },
  timestamp: {
    fontSize: 12,
    color: '#999',
    marginLeft: 10,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  emptyText: {
    fontSize: 16,
    color: '#666',
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.5)',
  },
  dropdown: {
    position: 'absolute',
    backgroundColor: '#fff',
    borderRadius: 8,
    padding: 8,
    shadowColor: '#000',
    shadowOffset: {
      width: 0,
      height: 2,
    },
    shadowOpacity: 0.25,
    shadowRadius: 3.84,
    elevation: 5,
    minWidth: 120,
  },
  dropdownOption: {
    paddingVertical: 12,
    paddingHorizontal: 16,
    borderBottomWidth: 1,
    borderBottomColor: '#f0f0f0',
  },
  dropdownOptionText: {
    fontSize: 16,
    color: '#333',
  },
  deleteOption: {
    borderBottomWidth: 0,
  },
  deleteOptionText: {
    color: '#ff3b30',
  },
});