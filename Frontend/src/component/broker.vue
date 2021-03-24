<template>
	<h2>{{ displayName }}</h2>
</template>
<script lang="ts">
import { Vue } from 'vue-class-component';
import { BrokerDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

export default class Broker extends Vue {
	@Prop()
	broker: BrokerDto | undefined;

	get displayName() {
		return this.broker?.displayName;
	}
}
</script>
