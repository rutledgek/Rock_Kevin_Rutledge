import { Guid } from "@Obsidian/Types";
import { getTopic, ITopic, ServerFunctions } from "@Obsidian/Utility/realTime";
import { AttendanceUpdatedMessageBag } from "@Obsidian/ViewModels/Event/attendanceUpdatedMessageBag";

export type TopicListeners = {
    onAttendanceUpdated: (bag: AttendanceUpdatedMessageBag) => void
};

export type GroupAttendanceTopic = ITopic<ServerFunctions<unknown>> & {
    attendanceOccurrenceGuid: Guid
};

async function getGroupAttendanceTopic(groupGuid: Guid, attendanceOccurrenceGuid: Guid): Promise<GroupAttendanceTopic> {
    const topic = await getTopic("Rock.RealTime.Topics.EntityUpdatedTopic");

    Object.defineProperty(topic, "attendanceOccurrenceGuid", {
        value: attendanceOccurrenceGuid,
        writable: false
    });

    return topic as GroupAttendanceTopic;
}

export async function startRealTime(groupGuid: Guid, attendanceOccurrenceGuid: Guid, listeners: TopicListeners): Promise<GroupAttendanceTopic> {
    const topic = await getGroupAttendanceTopic(groupGuid, attendanceOccurrenceGuid);

    topic.onDisconnected(async () => {
        await startRealTime(groupGuid, attendanceOccurrenceGuid, listeners);
    });

    setUpTopicListeners(topic, listeners);

    // Extra logic that needs to happen on every connection goes here.

    // if (!isReconnecting) {
    //     // One time logic goes here.
    // }

    return {
        ...topic,
        attendanceOccurrenceGuid: attendanceOccurrenceGuid
    };
}

export async function stopRealTime(topic: GroupAttendanceTopic): Promise<void> {
    //topic.server.stopMonitoringAttendanceOccurrence(topic.attendanceOccurrenceGuid);
}

function setUpTopicListeners(topic: GroupAttendanceTopic, listeners: TopicListeners): void {
    topic.on("attendanceUpdated", listeners.onAttendanceUpdated);
}