<template>
	<div class="card" style="width: 20rem">
		<div class="card-body">
			<h4>{{ broker.displayName }}</h4>
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { BrokerDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'BrokerCard',
})
export default class BrokerCard extends Vue {
	@Prop()
	broker!: BrokerDto;
}
</script>
