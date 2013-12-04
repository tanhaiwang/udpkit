LOCAL_PATH := $(call my-dir)  
include $(CLEAR_VARS)  
LOCAL_LDLIBS := -llog  
LOCAL_MODULE    := udpkit_android
LOCAL_SRC_FILES := ../../../src/udpkit.lib/common.cpp ../../../src/udpkit.lib/socket.cpp ../../../src/udpkit.lib/timers.cpp
include $(BUILD_SHARED_LIBRARY) 