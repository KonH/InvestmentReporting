<template>
	<calendar-view target-category="Bond Coupon"/>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import CalendarView from '@/view/calendar/calendarView.vue';

@Options({
	name: 'CouponsView',
	components: {
		CalendarView,
	},
})
export default class CouponsView extends Vue {}
</script>
