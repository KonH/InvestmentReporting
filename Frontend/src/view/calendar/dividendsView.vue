<template>
	<calendar-view target-category="Share Dividend" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import CalendarView from '@/view/calendar/calendarView.vue';

@Options({
	name: 'DividendsView',
	components: {
		CalendarView,
	},
})
export default class DividendsView extends Vue {}
</script>
