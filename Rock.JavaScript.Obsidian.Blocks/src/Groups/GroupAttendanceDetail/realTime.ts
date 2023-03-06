import { Guid } from "@Obsidian/Types";
import { getTopic, ITopic } from "@Obsidian/Utility/realTime";
import { GroupAttendanceDetailUpdateAttendeeRequestBag } from "@Obsidian/ViewModels/Blocks/Groups/GroupAttendanceDetail/groupAttendanceDetailUpdateAttendeeRequestBag";

interface IGroupAttendanceTopic {
    markAttendance(bag: GroupAttendanceDetailUpdateAttendeeRequestBag);
    startMonitoringAttendanceOccurrence(attendanceOccurrenceGuid: Guid);
    stopMonitoringAttendanceOccurrence(attendanceOccurrenceGuid: Guid);
}

const topicName = "Rock.RealTime.Topics.GroupAttendanceTopic";

export type TopicListeners = {
    onUpdateAttendance: (personGuid: Guid, didAttend: boolean) => void
};

export type GroupAttendanceTopic = ITopic<IGroupAttendanceTopic> & {
    attendanceOccurrenceGuid: Guid
};

async function getGroupAttendanceTopic(attendanceOccurrenceGuid: Guid): Promise<GroupAttendanceTopic> {
    const topic = await getTopic<IGroupAttendanceTopic>(topicName);

    Object.defineProperty(topic, "attendanceOccurrenceGuid", {
        value: attendanceOccurrenceGuid,
        writable: false
    });

    return topic as GroupAttendanceTopic;
}

export async function startRealTime(attendanceOccurrenceGuid: Guid, listeners: TopicListeners): Promise<GroupAttendanceTopic> {
    const topic = await getGroupAttendanceTopic(attendanceOccurrenceGuid);

    topic.onDisconnected(async () => {
        await startRealTime(attendanceOccurrenceGuid, listeners);
    });

    setUpTopicListeners(topic, listeners);

    await topic.server.startMonitoringAttendanceOccurrence(attendanceOccurrenceGuid);

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
    topic.server.stopMonitoringAttendanceOccurrence(topic.attendanceOccurrenceGuid);
}

function setUpTopicListeners(topic: ITopic<IGroupAttendanceTopic>, listeners: TopicListeners): void {
    topic.on("updateAttendance", listeners.onUpdateAttendance);
}