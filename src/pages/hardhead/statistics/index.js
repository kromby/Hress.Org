/*
    SIDEBAR
        Filter
        Hver ár

    MAIN
        Samantekt á tímabili
            Upphaf og endir
            Fjöldi kvölda
            Fjöldi gesta
            Fjöldi unique gesta
            Fjöldi unique gestgjafa
            Fjöldi unique leikara
        [X] - Kvöld með flesta gesti
        [X] - Oftast mætt
        [X] - Oftast haldið
        [ ] - Flest harðhausaverðlaun
        [X] - Vinsælasti leikarinn
        [ ] - Flest kvöld í röð
        [ ] - Gestgjafi með flest kvöld á einu ári
*/

import { useEffect } from 'react';
import GuestStats from './guestStats';
import HostStats from './hostStats';
import ActorStats from './actorStats';
import AttendanceStats from './attendanceStats';
import { ErrorBoundary } from 'react-error-boundary';

const Statistics = () => {
    useEffect(() => {
        document.title = "Tölfræði Harðhausa | Hress.Org";
    }, [])

    return (
        <div id="main">
            <ErrorBoundary
                FallbackComponent={<div>Það vantar einhvern hressleika hér!</div>}
            >
                <GuestStats />
                <HostStats />
                <ActorStats />
                <AttendanceStats />
            </ErrorBoundary>
        </div>
    )
}

export default Statistics;